import { Component } from '@angular/core';
import { Application } from '../../models/Application';
import { CompositeApp, LoadedCompositeApp } from '../../models/composites';

@Component({
    styleUrls: ['./plugins.component.scss'],
    templateUrl: './plugins.component.html'
})
export class PluginsComponent {

    public constructor(
        private application: Application) {
    }

    public get loadedApplications(): LoadedCompositeApp[] {
        return this.application.loadedCompositeApps;
    }  

    public get displayLoadMessage(): boolean {
        return  !this.application.loadCompositeApp || this.application.loadedCompositeApps.length === 0;
    }

    // public closeResource(resource: LoadedResource) {
    //     this.application.removeResourceFromConn(resource);
    // }
    
}