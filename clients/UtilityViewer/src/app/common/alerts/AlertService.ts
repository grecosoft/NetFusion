import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { SimpleAlert } from './models';

// Encapsulates methods for notifying the user.  Additional
// methods can be added to this central location.
@Injectable()
export class AlertService {

    public constructor(
        private snackBar: MatSnackBar) {

    }

    public displaySimpleAlert(alert: SimpleAlert) {

        this.snackBar.open(alert.message, alert.action || 'OK', {
            duration: alert.duration || 500,
            verticalPosition: "top"
          });
    }
}