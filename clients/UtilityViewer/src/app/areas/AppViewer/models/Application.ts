import { Injectable } from '@angular/core';
import { CompositeApp, LoadedCompositeApp, CompositeAppSettings } from './composites';
import { CompositeStructure } from './api.models-composite';
import { HttpClient } from '@angular/common/http';
import { PersistenceService } from 'angular-persistence';

@Injectable()
export class Application {

    public compositeAppSettings: CompositeAppSettings;

    private _loadedCompositeApps: LoadedCompositeApp[] = [];

    constructor(
        private http: HttpClient,
        private persistenceService: PersistenceService) {
    }
 
    public loadCompositeApp(application: CompositeApp) {
  
        this.compositeAppSettings.getCompositeStructure(application)
            .subscribe((structure: CompositeStructure) => {

                let loadedComposite = new LoadedCompositeApp(
                    application,
                    structure);

                this._loadedCompositeApps.push(loadedComposite);
            });
    }

    public get loadedCompositeApps(): LoadedCompositeApp[] {
        return this._loadedCompositeApps;
    }

    public restoreState() {

        this.compositeAppSettings = new CompositeAppSettings(this.http, this.persistenceService);
        this.compositeAppSettings.restoreState();
    }
}


