// Angular Types:
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl, AbstractControl } from '@angular/forms';

import * as _ from 'lodash';

// UI Types:
import { MatTableDataSource } from '@angular/material';

// Application Types:
import { Application} from '../../models/Application';
import { ApiConnection } from '../../models/connections';
import { NavigationService } from '../../../../common/navigation/NavigationService';
import { Notification } from '../../models/notifications';
import { ConfirmationService } from '../../../../common/confirmation/ConfirmationService';
import { ConfirmResponseTypes, ConfirmSettings } from '../../../../common/confirmation/models';
import { AlertService } from '../../../../common/alerts/AlertService';

@Component({
    styleUrls: ['connections.component.scss'],
    templateUrl: 'connections.component.html',
})
export class ConnectionsComponent implements OnInit {

    public connectionEntry: FormGroup;
    public existingConnection: ApiConnection;

    public connectionColumns = ['name', 'address', 'actions'];
    public connections: MatTableDataSource<ApiConnection>;
    private _isAddingNewConn = false;

    public constructor(
        private application: Application,
        private navigation: NavigationService,  
        private confirmation: ConfirmationService,
        private alerting: AlertService,
        private formBuilder: FormBuilder) {
    }

    public ngOnInit() {
        this.createEntry();
        this.connections = new MatTableDataSource(this.application.connSettings.connections);
    }

    public get isEditingConnection(): boolean {
      return !this.application.connSettings.hasConnections || this.existingConnection != null || this._isAddingNewConn;
    }

    public get displayCancel() {
       return this.isEditingConnection && this.application.connSettings.hasConnections;
    }

    public get displayClose() {
      return !this.isEditingConnection || !this.application.connSettings.hasConnections;
    }

    // List of configured notifications that can be selected from and associated
    // with the connection.
    public get notificationList(): Notification[] {
        return this.application.notificationSettings.notifications;
    }

    public saveConnection() {

        // Update existing connection being edited for the form state or
        // create new connection.
        let formModel = this.connectionEntry.value;
        let conn = Object.assign(this.existingConnection || {}, formModel);

        this.application.connSettings.saveConnection(conn);
        this.resetEntryState();
    }

    public cancelEdit() {
        this.resetEntryState();
    }

    private resetEntryState() {
        this.existingConnection = null;
        this._isAddingNewConn = false;
        this.connectionEntry.reset();    
    }

    // Update the form group with the value of the selected connection.
    public editConnection(connection: ApiConnection) {
        this.existingConnection = connection;
        this.connectionEntry.reset(this.existingConnection);
    }

    public deleteConnection(connection: ApiConnection) {

        let confirmation = new ConfirmSettings(
            'Delete Connection', 
            `Are you sure you want to delete connection: ${connection.name}?`);

        confirmation.confirmText = "Delete";

        this.confirmation.verifyAction(confirmation).subscribe((answer) => 
        {
            if (answer == ConfirmResponseTypes.ActionConfirmed) {

               this.application.deleteConnection(connection);
               this.connections.data = this.application.connSettings.connections;

               this.alerting.displaySimpleAlert({ message: 'Connection Deleted', duration: 800 });
            }
        });
    }

    public addConnection() {
        this._isAddingNewConn = true;
        this.connectionEntry.reset();    
    }

    private createEntry() {

        this.connectionEntry = this.formBuilder.group({
            name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(20)] ],
            address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(50)] ],
            entryPath: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(50)] ],
            notificationIds: [[]]
        });
    }

    public isInputInvalid(fieldName: string): boolean {
        return this.connectionEntry.get(fieldName).invalid;
    }

    public get nameInput(): AbstractControl {
        return this.connectionEntry.get('name');
    }

    // Navigates the user to the entry resource for the selected connection.
    public navigateToConnEntries(connection: ApiConnection) {
        this.application.setSelectedConnection(connection);
        this.navigation.navigateToMenuItemById('ENTRIES', connection);
    }
}    
