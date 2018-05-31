// Angular Types:
import { HttpResponse } from '@angular/common/http';

// Library Types:
import * as _ from 'lodash';
import * as shortid from 'shortid';

// Common Types:
import { SelectItem, SelectList } from '../../common/selection';
import { SettingsService } from '../../../common/settings/SettingsService';

// Application Types:
import { Application } from './Application';
import { ApiConnection } from '../models/connections';
import { LoadedResource } from './resources';
import { NotificationConfigSettings } from './settings';

// Class that is delegated to from the area's root Application instance responsible
// from managing a list of notifications applied to a server response.
export class NotificationSettings {

    private _configSettings: NotificationConfigSettings;

    public sourceTypes: SelectList;

    // Lookup for a strategy function used to return the value associated with a notification.
    public notificationStrategies: {[sourceType: number]: (
        notification: Notification, 
        response: HttpResponse<any>, 
        resource: LoadedResource) => any};

    public constructor(
        private application: Application,
        private settingsService: SettingsService) {

            this.sourceTypes = this.getSourceTypes();
            this.setNotificationStrategies();
    }

    public get notifications(): Notification[] {
        return this._configSettings.notifications || [];
    }

    // Methods determining if a given notification applies to a received response
    // for a specific type of configured notification.
    private setNotificationStrategies() {
        this.notificationStrategies = {};

        this.notificationStrategies[SourceTypes.HeaderValue] = (n, rsp, res) => {
            
            // Check if there is a header value matching the source name of the notification.
            let headerValue = rsp.headers.get(n.sourceName);
            if (!headerValue) {
                return null;
            }

            // If the notification has a specific source value specified determine if
            // the value of the header matches.
            if (n.sourceValue) {
                return n.sourceValue === headerValue;
            }

            return headerValue;
        };

        this.notificationStrategies[SourceTypes.EmbeddedResource] = (n, rsp, res) => {
            let embeddedRes = res.embedded && res.embedded[n.sourceName];
            if (embeddedRes) {
                return embeddedRes.resource;
            }
            return null;
        };
    }

    public restoreState() {
        this._configSettings = this.settingsService.read<NotificationConfigSettings>(NotificationConfigSettings.storeKey);
        this._configSettings = this._configSettings || new NotificationConfigSettings();
    }

    public saveNotification(notification: Notification) {

        if (!notification.id) {
            notification.id = shortid.generate();
            this.notifications.push(notification);
        }

        this.saveNotifications();
    }

    public deleteNotification(notification: Notification) {
        let idx = this.notifications.indexOf(notification);

        if (idx > -1) {
            this.notifications.splice(idx, 1);
            this.removeFromConnections(notification);
            this.saveNotifications();
        }
    }

    private removeFromConnections(notification: Notification) {

        // Remove notification from all assigned connections.
        let connSettings = this.application.connSettings;

        _.forEach(connSettings.connections, c => {
            connSettings.removeNotification(c, notification);
        });
    }

    private saveNotifications() {
        this.settingsService.save(NotificationConfigSettings.storeKey, this._configSettings);
    }

    public get hasNotifications() {
        return this.notifications.length > 0;
    }

    private getSourceTypes(): SelectList {
        
        let list = new SelectList();
        list.addItem(SourceTypes.HeaderValue, 'Header Value');
        list.addItem(SourceTypes.EmbeddedResource, 'Embedded Resource');

        return list;

    }

    public setNotifications(loadedResource: LoadedResource, response: HttpResponse<any>, notification: Notification) {

        let strategy = this.notificationStrategies[notification.sourceType];
        if (strategy) {
            let sourceValue = strategy(notification, response, loadedResource);

            if (!sourceValue) {
                return;
            }

            let responseNotification = new ResponseNotification(
                notification.name,
                this.sourceTypes[notification.sourceType], 
                notification.sourceName, 
                sourceValue,
                notification.sourceType != SourceTypes.EmbeddedResource);

            loadedResource.addLastNotification(responseNotification);
        }
    }

    public getConnectionNotifications(connection: ApiConnection): Notification[] {
        
        return _.map(connection.notificationIds, (id: string) => {
            return _.find(this.notifications, n => n.id === id);
        });
    }
}

export class Notification {
    
    public id: string;
    public name: string;
    public sourceType: SourceTypes;
    public sourceName: string;
    public sourceValue: string;
}

export enum SourceTypes {
    HeaderValue = 1,
    EmbeddedResource = 2
}

export class ResponseNotification {
    public constructor(
        public notificationName: string,
        public sourceType: string,
        public sourceName: string,
        public sourceValue: any,
        public displayInline: boolean) {

    }
}

