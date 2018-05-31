// Angular Types:
import { Injectable} from '@angular/core';

// Library Types:
import * as _ from 'lodash';
import { Observable, Subject } from 'rxjs';

// Common Types:
import { RequestClientFactory } from '../../../common/client/RequestClientFactory';
import { Link, IHalResource, IHalEntryPointResource } from '../../../common/client/Resource';
import { ApiRequest, ApiResponse } from '../../../common/client/Request';
import { ApiConnection, ConnectionSettings } from './connections';
import { SettingsService } from '../../../common/settings/SettingsService';

// Application Types:
import { LoadedResource, SelectedResourceLink } from './resources';
import { Notification, NotificationSettings } from './notifications';
import { LoadedResourcesConfigSettings, ApplicationSettings, ApplicationConfigSettings } from './settings';
import { EnvironmentSettings } from './environments';


// The root service representing the main entry point for the HalViewer application area.
@Injectable()
export class Application {

    // Child object instances responsible for managing a specific application feature.
    public applicationSettings: ApplicationSettings;
    public connSettings: ConnectionSettings;
    public notificationSettings: NotificationSettings;
    public environmentSettings: EnvironmentSettings;

    // The current selections:
    public selectedConn: ApiConnection;
    public selectedEntryResource: LoadedResource;
    public selectedResourceTabIndex = 0;

    // A dictionary containing a list of resources that have been loaded using a given connection.
    private _connLoadedResources: {[connId: string]: LoadedResource[]};

    // Subject that is set to notify any subscribers of selection changes.
    private _connSelected: Subject<ApiConnection>;
    private _responseReceived: Subject<IHalResource>;

    constructor(
        private clientFactory: RequestClientFactory,
        private settingsService: SettingsService) {

            this._connLoadedResources = {};

            this._connSelected = new Subject<ApiConnection>();
            this._responseReceived = new Subject<IHalResource>();
    }

    // This method should be called when the application is bootstrapped to restore any application
    // state from local storage.
    public restoreState() {

        // Restore the saved application settings or use defaults.
        this.applicationSettings = this.settingsService.read<ApplicationSettings>(
            ApplicationConfigSettings.storeKey);

        this.applicationSettings = this.applicationSettings || new ApplicationSettings();

        // Create and restore connection settings.
        this.connSettings = new ConnectionSettings(this.clientFactory, this.settingsService);
        this.connSettings.restoreState();

        // Create and restore notification settings.
        this.notificationSettings = new NotificationSettings(this, this.settingsService);
        this.notificationSettings.restoreState();

        // Determine the last selected connection and restore
        // prior loaded resources.
        this.selectedConn = this.connSettings.getLastSelectedConnection();
        this.restoreLoadedResources();
    }

    public updateSettings(settings: ApplicationSettings) {

        this.applicationSettings = settings;

        this.settingsService.save(
            ApplicationConfigSettings.storeKey, 
            settings);

        this.saveLoadedResources();
    }

    // Updates the currently selected connection and invokes an observable to allow
    // application components to take action.  The observable is invoked after the
    // entry resource for the selected connection is loaded.
    public setSelectedConnection(connection: ApiConnection) {
        this.selectedConn = connection;

        this.connSettings.getEntryResource(connection)
            .subscribe((entryResource: IHalEntryPointResource) => {
  
                this.selectedEntryResource = new LoadedResource(_.cloneDeep(entryResource));

                this.selectedResourceTabIndex = 0;
                this._connSelected.next(connection);
            });
    }

    // Allows clients to subscribe to be notified when the selected connection changes.
    public get whenConnectionSelected(): Observable<ApiConnection> {
        return this._connSelected.asObservable();
    }

    // Allows client so subscribe to be notified when an execution result is completed.
    public get whenResponseReceived(): Observable<IHalResource> {
        return this._responseReceived.asObservable();
    }

    // All of the resources that were loaded on the currently selected connection.
    public get selectedConnResources(): LoadedResource[] {
        if (this.selectedConn == null) {
            return [];
        }

        return this._connLoadedResources[this.selectedConn.id] || [];
    }

    // Execute a relation(Link) associated with a given resource.
    public executeResourceLink(relation: SelectedResourceLink) {

        // Get the client associated connection and create a request from the Link
        // to be submitted to the server.

        let client = this.clientFactory.getClient(this.selectedConn.id);
 
        let request = ApiRequest.fromLink(relation.link.associatedLink, 
            (config) => config.withRouteValues(relation.linkParams || {}));
        
        if (relation.content) {
            request.withContent(relation.content);
        }

        // Execute the request and associated the returned resource with
        // the connection.
        client.send<IHalResource>(request).map((apiResponse) => {
            
            let httpMethod = relation.link.method;
  
            if (apiResponse.response.ok && this.hasHalResourceStructure(apiResponse.content)) {

                // Convert to a resource type specifically used by the UI containing a subset 
                // of the properties that should be displayed to the user.  the relation tag
                // is used to identify the link used to load the resource and used by the UI.
                let relationTag =  `(${relation.link.relName}) ${request.requestUri}`;
                let resource = new LoadedResource(apiResponse.content, relation.link.relName, relationTag);

                this.associateResourceWithConn(resource);
                resource.setLastResponse(apiResponse.content);
                this.populateResponseNotifications(resource, apiResponse);

            } else if (httpMethod === "DELETE" && apiResponse.response.ok) {
                // If all was okay, remove the resource since it was deleted.  This will 
                // result in resource's tab being removed.
                this.removeResourceFromConn(relation.loadedResource);

            } else {
                relation.loadedResource.setLastResponse(apiResponse.content);
                this.populateResponseNotifications(relation.loadedResource, apiResponse);
            }

            this._responseReceived.next(apiResponse.content);
        }).subscribe();
    }

    private hasHalResourceStructure(value: any): boolean {
        return _.has(value, '_links') || _.has(value, '_embedded');
    }

    public deleteConnection(connection: ApiConnection) {
        this.connSettings.deleteConnection(connection);

        delete this._connLoadedResources[connection.id];
        this.saveLoadedResources()
    }

    private associateResourceWithConn(resource: LoadedResource) {

        if (!this._connLoadedResources[this.selectedConn.id]) {
            this._connLoadedResources[this.selectedConn.id] = [];
        }

        let connResources = this._connLoadedResources[this.selectedConn.id];
        connResources.push(resource);

        this.selectedResourceTabIndex = connResources.length - 1;
        this.saveLoadedResources();
    }

    // Removes a resource that was loaded on the currently selected connection.
    public removeResourceFromConn(resource: LoadedResource) {
        let idx = this.selectedConnResources.indexOf(resource);
        if (idx > -1) {
            this.selectedConnResources.splice(idx, 1);

            // Remove connection key to resource lookup if no associated
            // resources to prevent storing empty array.
            if (this.selectedConnResources.length == 0) {
                delete this._connLoadedResources[this.selectedConn.id];
            }

            this.saveLoadedResources();
        }
        this.selectedResourceTabIndex = 0;
    }

    // Called when an embedded resource is selected to be acted on.
    // This results in a new tab to be created for the embedded resource.
    public openEmbeddedResource(resource: LoadedResource) {
        this.associateResourceWithConn(resource);
    }

    private populateResponseNotifications(resource: LoadedResource, apiResponse: ApiResponse<IHalResource>) {
        let connNotifications = this.notificationSettings.getConnectionNotifications(this.selectedConn);

        _.forEach(connNotifications, (notification: Notification) => {            
            this.notificationSettings.setNotifications(resource, apiResponse.response, notification);
        });
    }

    public updateResource(link: Link, resource: any): Observable<any> {

        let request = ApiRequest.fromLink(link)
            .withContent(resource);

        let client = this.clientFactory.getClient(this.selectedConn.id);

        return client.send<IHalResource>(request).map((resp) => {
            return resp.content;
        });
    }

    private restoreLoadedResources() {

         // Load the last resources the user had queried:
         let loadedResourceConfig = this.settingsService.read<LoadedResourcesConfigSettings>(
            LoadedResourcesConfigSettings.storeKey);

        if (loadedResourceConfig) {
            this._connLoadedResources = loadedResourceConfig.resources || {};
        }
    }

    private saveLoadedResources() {

        let resourceConfigSettings = new LoadedResourcesConfigSettings({});

        if (this.applicationSettings.saveOpenedTabs) {
            resourceConfigSettings = new LoadedResourcesConfigSettings(this._connLoadedResources);
        }

        this.settingsService.save(LoadedResourcesConfigSettings.storeKey, resourceConfigSettings);
    }
}


