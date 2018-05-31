// Library Types:
import * as _ from 'lodash';
import { Subscription } from 'rxjs';

// Angular Types:
import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material';

// Application Types:
import { Application } from '../../models/Application';
import { Notification, SourceTypes } from '../../models/notifications';
import { SelectItem, SelectList } from '../../../common/selection';
import { ConfirmSettings, ConfirmResponseTypes } from '../../../../common/confirmation/models';
import { ConfirmationService } from '../../../../common/confirmation/ConfirmationService';


@Component({
    styleUrls: ['./notifications.component.scss'],
    templateUrl: './notifications.component.html'
})
export class NotificationsComponent implements OnDestroy {

    public notificationEntry: FormGroup;
    public existingNotification: Notification;

    public notificationColumns = ['name', 'sourceType', 'sourceName', 'sourceValue', 'actions'];
    public notifications: MatTableDataSource<NotificationViewModel>;
    public isAddingNewNotification = false;

    private editSubscription: Subscription;

    public constructor(
        private application: Application,
        private confirmation: ConfirmationService,
        private formBuilder: FormBuilder) {
    }
    
    public ngOnInit() {
        this.createEntry();
        this.notifications = new MatTableDataSource(this.getNotificationViewModels());
    }

    public ngOnDestroy() {
        this.editSubscription.unsubscribe();
    }

    public get sourceTypes(): SelectList {
        return this.application.notificationSettings.sourceTypes;
    }

    public get isEditingNotification(): boolean {
        return !this.application.notificationSettings.hasNotifications || this.existingNotification != null || this.isAddingNewNotification;
    }

    public get displayCancel() {
        return this.isEditingNotification && this.application.notificationSettings.hasNotifications;
     }
 
    public get displayClose() {
        return !this.isEditingNotification || !this.application.notificationSettings.hasNotifications;
    }

    public saveNotification() {

        // Update existing notification being edited for the form state or
        // create new notification.
        let formModel = this.notificationEntry.value;
        let notification = Object.assign(this.existingNotification || {}, formModel);

        this.application.notificationSettings.saveNotification(notification);
        this.notifications.data = this.getNotificationViewModels();
        this.resetEntryState();
    }

    public cancelEdit() {
        this.resetEntryState();
    }

    private resetEntryState() {
        this.existingNotification = null;
        this.isAddingNewNotification = false;
        this.notificationEntry.reset();    
    }

    public editNotification(viewModel: NotificationViewModel) {
        this.existingNotification = viewModel.notification;
        this.notificationEntry.reset(this.existingNotification);
    }

    public deleteNotification(viewModel: NotificationViewModel) {

        let confirmation = new ConfirmSettings(
            'Delete Notification', 
            `Are you sure you want to delete notification: ${viewModel.name}?`);

        confirmation.confirmText = "Delete";

        this.confirmation.verifyAction(confirmation).subscribe((answer) => {

            if (answer == ConfirmResponseTypes.ActionConfirmed) {

                this.application.notificationSettings.deleteNotification(viewModel.notification);
                this.notifications.data = this.getNotificationViewModels();
            }
        });
    }

    public isInputInvalid(fieldName: string): boolean {
        return this.notificationEntry .get(fieldName).invalid;
    }

    public addNotification() {
        this.isAddingNewNotification = true;
        this.notificationEntry.reset();    
    }
   
    private createEntry() {
        this.notificationEntry = this.formBuilder.group({
            name: [null, [Validators.required, Validators.minLength(5), Validators.maxLength(50)] ],
            sourceType: [null, Validators.required],
            sourceName: [null, [Validators.required, Validators.minLength(1), Validators.maxLength(50)] ],
            sourceValue: [null, [Validators.minLength(1), Validators.maxLength(50)]]
        });

        this.editSubscription = this.notificationEntry.valueChanges.subscribe((value: Notification) => {

            let sourceValue = this.notificationEntry.get("sourceValue");
            if (value.sourceType == SourceTypes.EmbeddedResource && sourceValue.enabled) {
               sourceValue.disable({emitEvent: false});
            } else if (value.sourceType == SourceTypes.HeaderValue && sourceValue.disabled) {
                sourceValue.enable({emitEvent: false});
            }
        });
    }

    private getNotificationViewModels(): NotificationViewModel[] {
        return _.map(
            this.application.notificationSettings.notifications, 
            n => { return new NotificationViewModel(n, this.sourceTypes)});      
    }
} 

export class NotificationViewModel {

    public constructor(
        public notification: Notification,
        private sourceTypes: SelectList) {

    }

    public get name(): string {
        return this.notification.name;
    }

    public get sourceType(): string {
        return this.sourceTypes[this.notification.sourceType];
    }

    public get sourceName(): string {
        return this.notification.sourceName;
    }

    public get sourceValue(): string {
        return this.notification.sourceValue;
    }
}