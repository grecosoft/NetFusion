import { Component, Input, OnInit, OnChanges } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

import { LoadedResource, SelectedResourceLink, ResourceLinkViewModel } from '../../models/resources';
import { Application } from '../../models/Application';
import { Observable, Subscription, Subject } from 'rxjs';
import { ConfirmationService } from '../../../../common/confirmation/ConfirmationService';
import { ConfirmSettings, ConfirmResponseTypes } from '../../../../common/confirmation/models';

@Component({
    selector: 'resource-viewer',
    styleUrls: ['./resource-viewer.component.scss'],
    templateUrl: './resource-viewer.component.html'
})
export class ResourceViewerComponent implements OnChanges {

    @Input('loadedResource')
    public loadedResource: LoadedResource;

    constructor(
        private application: Application,
        private confirmation: ConfirmationService) {
    }

    public resourceJson: string;

    public ViewOptions = ViewOptions;
    public selectedViewOption = ViewOptions.View;

    public viewOptionSelected(options: ViewOptions) {
        this.selectedViewOption = options;
    } 

    public ngOnChanges() {
        this.resourceJson = JSON.stringify(this.loadedResource.resource, undefined, 4);
    }

    public resourceLinkSelected(selectedRelation: SelectedResourceLink) {

        if (this.sendContent(selectedRelation)) {
            //selectedRelation.content = JSON.parse(this.resourceJson); 
        }

        if (this.isConfirmRequired(selectedRelation)) {
            this.confirmAction(selectedRelation, () => {
                this.application.executeResourceLink(selectedRelation);
            });
            return;
        }

        this.application.executeResourceLink(selectedRelation);
    }

    private confirmAction(selectedRelation: SelectedResourceLink, action: () => void) {

        let confirmation = new ConfirmSettings(
            'Alter Resource?', 
            `
                Are you sure you want to apply http method: ${selectedRelation.link.method} 
                to resource: ${selectedRelation.link.resourceUrl} ?`);

        // Set the text of the confirmation button to the HTTP Method.
        confirmation.confirmText = selectedRelation.link.method;

        this.confirmation.verifyAction(confirmation).subscribe((answer) => 
        {
            if (answer == ConfirmResponseTypes.ActionConfirmed) {

                action();
            }
        });
    }

    private isConfirmRequired(selectedRelation: SelectedResourceLink): boolean {

        const method = selectedRelation.link.method;
        return method === "PUT" || method === "DELETE";
    }

    private sendContent(selectedRelation: SelectedResourceLink) {
        const method = selectedRelation.link.method;
        return method === "PUT" || method === "POST";
    }

    public setResource() {
        this.loadedResource.resource = this.loadedResource.lastResponse;
        this.ngOnChanges();
    }
}

export enum ViewOptions {
    View = 1,
    Edit = 2
}