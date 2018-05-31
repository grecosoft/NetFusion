import { Component, Input, OnChanges } from '@angular/core';
import { ResponseNotification } from '../../models/notifications';
import { MatTableDataSource, MatTable, MatDialog } from '@angular/material';
import { JsonViewerDialog, DialogData } from '../../../../common/json-viewer/dialog/json-viewer.dialog';
import { JsonViewerService } from '../../../../common/json-viewer/JsonViewerService';

@Component({
    selector: 'notification-viewer',
    templateUrl: './notification-viewer.component.html',
    styles: ['./notification-viewer.component.scss']
})
export class NotificationViewerComponent implements OnChanges {

    @Input('notifications')
    public notifications: ResponseNotification[];

    public constructor(
        private jsonViewerService: JsonViewerService) {

    }

    public notificationList: MatTableDataSource<ResponseNotification>;
    public notificationColumns = ['name', 'sourceType', 'sourceName', 'sourceValue'];

    public ngOnChanges() {
        this.notificationList = new MatTableDataSource(this.notifications);
    }

    public displayResource(notification: ResponseNotification) {

        this.jsonViewerService.displayJson(notification.sourceName, notification.sourceValue);
    }

}