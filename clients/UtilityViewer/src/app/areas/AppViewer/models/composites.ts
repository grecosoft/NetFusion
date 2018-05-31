import * as shortid from 'shortid';
import * as _ from 'lodash';

import { PersistenceService, StorageType } from "angular-persistence";
import { Observable } from 'rxjs/Observable';
import { CompositeStructure, PluginDetails } from './api.models-composite';
import { HttpClient } from '@angular/common/http';

export class CompositeAppSettings
{
    public applications: CompositeApp[] = [];

    public constructor(
        private http: HttpClient,
        private persistenceService: PersistenceService) {
    }

    // Loads the saved connections from local storage and initializes the client-factory.
    public restoreState() {
        this.applications = this.persistenceService.get('applications', StorageType.LOCAL) || [];
    }

    public addApplication(application: CompositeApp) {
        
        application.id = shortid.generate();

        this.applications.push(application);
        this.saveApplications();
    }

    public deleteApplication(application: CompositeApp) {
        let idx = this.applications.indexOf(application);

        if (idx > -1) {
            this.applications.splice(idx, 1);
        }

        this.saveApplications();
    }

    public get hasApplications() {
        return this.applications.length > 0;
    }

    public saveApplications() {
        this.persistenceService.set('applications', this.applications, {type: StorageType.LOCAL});
    }

    public getCompositeStructure(application: CompositeApp): Observable<CompositeStructure> {
        return this.http.get<CompositeStructure>(application.address + '/structure');
    }

    public getPluginDetails(application: CompositeApp, pluginId: string): Observable<PluginDetails> {
        return this.http.get<PluginDetails>(application.address + '/plugins/' + pluginId);
    }
}

export class CompositeApp {
    public id: string;
    public name: string;
    public address: string;
}

export class LoadedCompositeApp {

    constructor(
        public compositeApp: CompositeApp,
        public structure: CompositeStructure) {
    }

    public get id(): string {
        return this.compositeApp.id;
    }

    public get name(): string {
        return this.compositeApp.name;
    }
}

export class PluginViewModel {
    public id: string;
    public name: string;
    public assembly: string;
    public type: string;
}

export class KnownTypeContractViewModel {
    public knownTypeName: string;
}

export class KnownTypeDefinitionViewModel {

    constructor(
        public definitionTypeName: string, 
        private discoveringPlugins: string[]) {

        }

    public get discoveringPluginNames(): string {
        return this.discoveringPlugins.join(' ,');
    }
}

export class PluginPropValue {
    public constructor(
        public id: string,
        public name: string,
        public assembly: string,
        public description: string,
        public sourceUrl: string,
        public docUrl: string) {
    }
}