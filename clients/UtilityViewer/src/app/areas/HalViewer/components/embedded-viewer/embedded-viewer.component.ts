import * as _ from 'lodash'
import { Component, Input, OnChanges, ViewChild } from '@angular/core';
import { LoadedResource } from '../../models/resources';
import { IHalResource } from '../../../../common/client/Resource';
import { MatTableDataSource } from '@angular/material';
import { Application } from '../../models/Application';
import { JsonViewerService } from '../../../../common/json-viewer/JsonViewerService';

// Component that displays the embedded resources of the current resource.  Technically, an 
// embedded resource can be recursive.  However returning deeply nested embedded resources is
// probably a bad idea and I have better things to be doing so this code assumes only one level
// of a nested resource or resource-collections.
@Component({
    selector: 'embedded-viewer',
    templateUrl: './embedded-viewer.component.html',
    styleUrls: ['./embedded-viewer.component.scss']
})
export class EmbeddedViewerComponent implements OnChanges  {

    @Input('loadedResource')
    public loadedResource: LoadedResource;

    public constructor(
        private application: Application,
        private jsonViewer: JsonViewerService) {

    }

    public embeddedResourceList: MatTableDataSource<EmbeddedResource>;
    public embeddedResourceColumns = ['name', 'type', 'structure', 'open'];

    public ngOnChanges() {

        this.embeddedResourceList = this.loadEmbeddedResources(this.loadedResource);
    }

    public loadEmbeddedResources(resource: LoadedResource): MatTableDataSource<EmbeddedResource> {
        let embeddedResources = _.map(resource.embedded, (r: any) => {
            return new EmbeddedResource(r);
        });

        return new MatTableDataSource(embeddedResources);
    }

    public openResource(embedded: EmbeddedResource) {
        this.application.openEmbeddedResource(embedded.loadedResource);
    }

    public displayResource(embedded: EmbeddedResource) {

        let displayStructure: any = embedded.loadedResource.resource;
        let embeddedName = this.displayResource.name;

        if (_.isArray(embedded.loadedResource)) {

            displayStructure = _.map(embedded.loadedResource, (lr: LoadedResource) => {
                embeddedName = lr.relationName;
                return lr.resource;
            });
        }

        this.jsonViewer.displayJson(embeddedName, displayStructure, true)
            .afterClosed().subscribe((resource: IHalResource) => {
                
                // User didn't select a resource contained within a resource collection.
                if (resource == null) {
                    return;
                }
            
            // Find the parent LoadedResource containing the selected inner resource.
            // Then open a new tab to view the resource.
            let selectedLoadedResource = _.find(embedded.loadedResource, 
                (lr: any) => lr.resource == resource);
            
                this.application.openEmbeddedResource(<any>selectedLoadedResource);
        });
    }
}

// Adds additional properties to an embedded resource pertaining to the view.
class EmbeddedResource {

    constructor(
        public loadedResource: LoadedResource) {
    }

    public get isObject(): boolean {
        return _.isPlainObject(this.loadedResource);
    }

    public get embeddedType(): string {
        return _.isArray(this.loadedResource) ? "Collection" : "Object";
    }

    public get relationName(): string {
        if (_.isArray(this.loadedResource)) {
            let resources = <any[]>this.loadedResource;
            return resources[0].relationName;
        }

        return this.loadedResource.relationName;
    } 

    public get canOpenInNewTab(): boolean {
        return !(_.isArray(this.loadedResource));
    }
}
