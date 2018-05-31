// Angular Types:
import * as _ from 'lodash';

// Library Types:
import * as shortid from 'shortid';
import { Observable } from 'rxjs';
import { PersistenceService, StorageType } from 'angular-persistence';

// Common Types:
import { IHalEntryPointResource } from '../../../common/client/Resource';
import { RequestClientFactory } from '../../../common/client/RequestClientFactory';
import { Link } from '../../../common/client/Resource';
import { SettingsService } from '../../../common/settings/SettingsService';

// Application Types:
import { Notification } from './notifications';
import { ConnectionConfigSettings } from './settings';

// Class that is delegated to from the area's root Application instance responsible
// for managing a list of URLs for REST/HAL based services.
export class ConnectionSettings {

    private _configSettings: ConnectionConfigSettings;

    public constructor(
        private clientFactory: RequestClientFactory,
        private settingsService: SettingsService) {
    }

    public get connections(): ApiConnection[] {
        return this._configSettings.connections;
    }

    public getConnection(connId: string): ApiConnection {
        let connIdx = _.findIndex(this._configSettings.connections, {id: connId});

        if (connIdx > -1) {
            return this._configSettings.connections[connId];
        }
        return null;
    }

    public getLastSelectedConnection(): ApiConnection {

        if (this.connections.length === 0) {
            return null;
        }

        let idx: number = -1;
        if (this._configSettings.lastSelectedConnId) {
            idx = _.findIndex(this.connections, {id: this._configSettings.lastSelectedConnId});
        }

        if (idx > -1) {
            return this.connections[idx];
        }

        if (this.connections.length > 0) {
            return this.connections[0];
        }
    }

    // Loads the saved connections from local storage and initializes the client-factory.
    // This should be called when the application area is bootstrapped.
    public restoreState() {
        this._configSettings = this.settingsService.read<ConnectionConfigSettings>(ConnectionConfigSettings.storeKey);
        this._configSettings = this._configSettings || new ConnectionConfigSettings();

        // This registers a connection and its associated address but no server
        // calls are made at this time.
        _.forEach(this._configSettings.connections, conn => {
            this.clientFactory.createClient(conn.id, conn.address);
        });
    }

    public saveConnection(connection: ApiConnection) {

        if (!connection.id) {
            connection.id = shortid.generate();
            this.connections.push(connection);
        }

        this.saveConfigurations();
    }

    // Deletes a connections and updated the application storage.
    public deleteConnection(connection: ApiConnection) {
        let idx = this.connections.indexOf(connection);

        if (idx > -1) {
            this.connections.splice(idx, 1);
            this.saveConfigurations();
        }
    }

    public get hasConnections() {
        return this.connections.length > 0;
    }

    private saveConfigurations() {
        this.settingsService.save(ConnectionConfigSettings.storeKey, this._configSettings);
    }

    // Loads the entry resource for the specified connection.  The entry resource allows
    // the user to start navigating the API provided by the REST/HAL service.
    public getEntryResource(connection: ApiConnection): Observable<IHalEntryPointResource> {
        this._configSettings.lastSelectedConnId = connection.id;
        this.saveConfigurations();

        return this.clientFactory.getEntryPointResource(connection.id, connection.entryPath);
    }

    // For a template based routed (i.e.:  /bla/{id}/details), returns an array of each
    // template parameter name.
    public getUrlParamNames(link: Link): string[] {
        let paramNames = link.href.match(/{(.*?)}/g);
        return _.map(paramNames, (name) => name.replace('{', '').replace('}', ''));
    }

    // Assigns a notification to a specific connection.  A notification can be configured to monitor
    // the HTTP Response for Headers or Embedded Hal Resources.
    public assignNotification(connection: ApiConnection, notification: Notification) {
        connection.notificationIds.push(notification.id);
        this.saveConfigurations();
    }

    public removeNotification(connection: ApiConnection, notification: Notification) {

        if (!connection.notificationIds) {
            return;
        }

        let idx = connection.notificationIds.indexOf(notification.id);
        
        if (idx > -1) {
            connection.notificationIds.splice(idx, 1);
            this.saveConfigurations();
        }
    }
} 

// Models a connection to a REST/HAL base server Web Api.
export class ApiConnection {
    public id: string;
    public name: string;
    public address: string;
    public entryPath: string;

    public notificationIds: string[] = [];
}