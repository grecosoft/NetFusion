import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, of} from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { ServiceApi } from './api/ServiceApi';
import { IHalEntryPointResource } from './common/rest-client/Resources';
import { NotificationService } from './common/services/NotificationService';
import { MatSnackBar } from '@angular/material';
import { Alert, NotificationLevel } from './common/services/services.models';

// Class representing the overall application.
@Injectable()
export class Application {

    public isLoggedIn = true;

    private _loginStateChange = new BehaviorSubject<boolean>(this.isLoggedIn);

    public constructor(
        private serviceApi: ServiceApi,
        private notifications: NotificationService,
        private snackBar: MatSnackBar) {
    }

    public bootstrap(): Observable<IHalEntryPointResource> {

        this.subscribeToNotifications();

        // Load the entry resource containing the links that can
        // be used to begin communicating with the application's Api.
        return this.serviceApi.loadServiceEntry(
           environment.apiBaseAddress,
           environment.apiEntryPath).pipe(
               map(entryRes => {
                   this.isLoggedIn = true;
                   this._loginStateChange.next(this.isLoggedIn);
                   return entryRes;
               })
           );
    }

    public get whenLoginStateChange(): Observable<boolean> {
        return this._loginStateChange.asObservable(); 
    }

    public loginUser() {
        this.serviceApi.ping().subscribe(() => {
            this.isLoggedIn = true;
            this._loginStateChange.next(this.isLoggedIn);
        });
    }

    public logOutUser() {
        this.isLoggedIn = false;
        this.serviceApi.clearJwtTokens();
        this._loginStateChange.next(this.isLoggedIn);
    }

    private subscribeToNotifications() {

        // Subscribe to the alerts observable and display a popup based
        // on the alert's data.
        this.notifications.alerts.subscribe(alert => {

            this.snackBar.dismiss();
            this.snackBar.open(alert.message, 'close',  {
                duration: 3000,
                verticalPosition: "bottom",
                horizontalPosition: "right",
                panelClass: this.getStyles(alert),
                data: alert
            });
        });
    }

    private getStyles(alert: Alert): string[] {
        let styles = ['boondocks-alert'];

        if (alert.level == NotificationLevel.Information) {
            styles.push('boondocks-alert-info');
        } else if(alert.level == NotificationLevel.Warning) {
            styles.push('boondocks-alert-warning');
        } else if (alert.level == NotificationLevel.Error) {
            styles.push('boondocks-alert-error');
        }

        return styles;
    }
}