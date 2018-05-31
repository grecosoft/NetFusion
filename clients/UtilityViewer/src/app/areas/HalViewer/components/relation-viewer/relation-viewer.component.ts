import * as _ from 'lodash';
import { Component, Input, Output, OnChanges, EventEmitter } from '@angular/core';
import { MatTableDataSource, MatTable } from '@angular/material';
import { FormGroup, FormControl, Validators } from '@angular/forms';

import { Link } from '../../../../common/client/Resource';
import { ResourceLinkViewModel, SelectedResourceLink, ParamValue, LoadedResource } from '../../models/resources';

// Displays a list of resource relations(Links) and allows the user to select for execution.  If the URL
// is template based, an entry form is created when an input for each template argument.
@Component({
    selector: 'relation-viewer',
    styleUrls: ['./relation-viewer.component.scss'],
    templateUrl: './relation-viewer.component.html'
})
export class RelationViewerComponent implements OnChanges {

    // The resource being acted on.
    @Input('loadedResource')
    public loadedResource: LoadedResource;

    // Event that is emitted when the user selects a link or after a
    // template based link is fully populated.
    @Output('relationSelected')
    public relationSelected: EventEmitter<SelectedResourceLink>;

    public constructor() {

        this.relationSelected = new EventEmitter<SelectedResourceLink>();
    }

    // The links contained on the resource used to make associated requests.
    public resourceLinks: MatTableDataSource<ResourceLinkViewModel>;
    public resourceLinkColumns = ['relName', 'resourceUrl', 'method'];

    // An input group used to specify values for a URL containing template parameters.
    public paramNameColumns = ['name', 'input'];
    public paramValueEntry: FormGroup = null;
    public paramValueInputs: MatTableDataSource<ParamValue>;

    private selectedRelationLink: ResourceLinkViewModel;    

    public ngOnChanges() {
        if (this.loadedResource) {
            this.populateResourceLinks(this.loadedResource);
        }
    } 

    // Creates a list of view models for each resource associated link.
    private populateResourceLinks(loadedResource: LoadedResource) {
        
        let viewModels: ResourceLinkViewModel[] = [];

        _.forOwn(loadedResource.links, (link: Link, relName: string) => {
            viewModels.push(new ResourceLinkViewModel(relName, link));
        });

        this.resourceLinks = new MatTableDataSource(viewModels);
    }

    public get hasParameters(): boolean {
        return this.paramValueInputs.data.length > 0;
    }

    // Called to populate a URL containing template parameters before
    // it can be submitted to the server.  Creates a form group for
    // each template parameter for entry.
    public populateUrl(resourceLink: ResourceLinkViewModel) {
        this.selectedRelationLink = resourceLink;
        let paramNames = this.getUrlParamNames(this.selectedRelationLink.associatedLink);

        this.createParamInputEntry(resourceLink, paramNames);
    }

    public get hasContentBody(): boolean {
        return this.selectedRelationLink.hasContentBody;
    }

    // Invoked when user selects a non-template URL.
    public executeUrl(resourceLink: ResourceLinkViewModel) {
        this.relationSelected.emit(
            new SelectedResourceLink(this.loadedResource, resourceLink, null));
    }

    // Invoked after the user has specified any URL template parameters:
    public executeUrlTemplate() {

        // Get the state of the entered parameter values from the form-group.
        let paramValues = Object.assign({}, this.paramValueEntry.value);

        let selectedResourceLink = new SelectedResourceLink(this.loadedResource, this.selectedRelationLink, paramValues);

        if (this.hasContentBody) {
            selectedResourceLink.content = paramValues.content;
            delete paramValues.content;
        }

        this.relationSelected.emit(selectedResourceLink);
    }

    // Creates an Angular input group and adds an input control for each template parameter.
    private createParamInputEntry(resourceLink: ResourceLinkViewModel, paramNames: string[]) {
        let group = new FormGroup({});
        let paramEntries: ParamValue[] = [];

        _.forEach(paramNames, pn => {
            let control = new FormControl(null, Validators.required);
            group.addControl(pn, control);

            paramEntries.push(new ParamValue(pn, control));
        });

        if (resourceLink.hasContentBody){
            let contentControl = new FormControl(null, Validators.required);
            group.addControl("content", contentControl);
        }

        this.paramValueEntry = group;
        this.paramValueInputs = new MatTableDataSource(paramEntries);
    }

    private getUrlParamNames(link: Link): string[] {
        let paramNames = link.href.match(/{(.*?)}/g);
        return _.map(paramNames, (name) => name.replace('{', '').replace('}', ''));
    }
}