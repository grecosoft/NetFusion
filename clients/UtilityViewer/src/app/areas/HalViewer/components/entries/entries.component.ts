import * as _ from 'lodash';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import { Application } from '../../models/Application';
import { ApiConnection  } from '../../models/connections';
import { SelectedResourceLink, LoadedResource } from '../../models/resources';
import { NavigationService } from '../../../../common/navigation/NavigationService';

// Displays a list of REST/HAL entry resource links.  This provides the entry point into
// a REST base service API.
@Component({
    templateUrl: './entries.component.html',
    styleUrls: ['./entries.component.scss']
})
export class EntriesComponent implements OnInit, OnDestroy  {

    public entryResource: LoadedResource;

    // Application Subscriptions:
    private _connSelectedSub: Subscription;
    private _responseReceivedSub: Subscription;
 
    public constructor(
        private application: Application,
        private navigation: NavigationService) {

    }

    public ngOnInit() {

        // Display the entry resource of the currently selected connection.
        if (this.application.selectedEntryResource) {
            this.entryResource = this.application.selectedEntryResource;
        }

        // Monitor for when the selected connection changes.
        this._connSelectedSub = this.application.whenConnectionSelected.subscribe(
            (conn: ApiConnection) => {
                this.entryResource = this.application.selectedEntryResource;
            });

        // Monitor for when a response is received.  In this case the response
        // will be the entry resource.  
        this._responseReceivedSub = this.application.whenResponseReceived
            .subscribe((resource) => {
                this.navigation.navigateToMenuItemById("RESOURCES");
            });
    }

    // Invoked when the user selects an entry link to navigate.
    public executeEntryLink(selectedRelation: SelectedResourceLink) {

        this.application.executeResourceLink(selectedRelation);
    }

    public get displayLoadMessage(): boolean {
        return this.entryResource == null;
    }

    public ngOnDestroy() {
        this._connSelectedSub.unsubscribe();
        this._responseReceivedSub.unsubscribe();
    }
}
