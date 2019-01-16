import { Injectable } from "@angular/core";
import { Observable, Observer } from "rxjs";
import { filter } from "rxjs/operators";
import { Notification, Alert, ResourceUpdate } from "./services.models";

// Service allowing one component to 
@Injectable()
export class NotificationService {

    // An internal observable to which all notification are emitted.
    private _notifications = new Observable<Notification>(observer => {
        this._notify = observer;
    });
    
    // Used to emit notifications to the notification observable.
    private _notify: Observer<Notification>;

    // Filtered observables by notification type to which consumers
    // can subscribe.
    public alerts: Observable<Alert>;
    public resourceUpdates: Observable<ResourceUpdate>;

    public constructor() {
        this.alerts = this._notifications.pipe (
            filter(evt => evt instanceof Alert)
        ) as Observable<Alert>;

        this.resourceUpdates = this._notifications.pipe (
            filter(evt => evt instanceof ResourceUpdate)
        ) as Observable<ResourceUpdate>;
    }

    // Publishes a notification to which consumers can subscribe.
    public send(notification: Notification) {
        if (this._notify != null) {
            this._notify.next(notification);
        }
    }
}




