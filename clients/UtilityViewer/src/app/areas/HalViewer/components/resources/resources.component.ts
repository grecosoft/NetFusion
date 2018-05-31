import { Component } from '@angular/core';
import { Application } from '../../models/Application';
import { LoadedResource } from '../../models/resources';


@Component({
    styles: ['./resources.component.scss'],
    templateUrl: './resources.component.html'
})
export class ResourcesComponent {

   // public selectedTabIdx = 0;

    public constructor(
        private application: Application) {
    }

    public get loadedResources(): LoadedResource[] {
        return this.application.selectedConnResources;
    }  

    public closeResource(resource: LoadedResource) {
        this.application.removeResourceFromConn(resource);
    }

}