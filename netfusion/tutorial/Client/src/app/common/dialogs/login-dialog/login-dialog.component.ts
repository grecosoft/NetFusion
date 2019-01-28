import { Component, Inject } from "@angular/core";
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Observable, Observer } from "rxjs";

import { LoginDialogData } from "./LoginDialogData";
import { appSettings } from 'src/app/app.settings';

// Component used to request login credentials from the user.
@Component({
    selector: 'login-dialog',
    templateUrl: './login-dialog.component.html'
})
export class LoginDialogComponent {
    public appName = appSettings.applicationName;
    public credentials: FormGroup;

    // The exposed observer that can be subscribed to detected
    // when the users has submitted a login attempt.
    public loginAttempted: Observable<LoginDialogData>;
    private loginObserved: Observer<LoginDialogData>

    constructor(
        fb: FormBuilder,
        @Inject(MAT_DIALOG_DATA) public data: LoginDialogData) {

            // Define and populate the form group with initial values:
            this.credentials = fb.group({
                username: [data.username, Validators.required],
                password: ['', Validators.required]
            });

            // Create observable to which caller can subscribe:
            this.loginAttempted = new Observable<LoginDialogData>((observer) => {
                this.loginObserved = observer;
            });
    }

    public message: string;

    public setMessage(value: string) {
        this.message = value;

        setTimeout(() =>{
            this.message = "";
        }, 3000)
    }

    public isInputInvalid(fieldName: string): boolean {
        return this.credentials.get(fieldName).invalid;
    }

    // Called when the user submits their credentials.  Notify the observer
    // of the login attempt.
    public onSubmit() {
        
        this.data.username = this.credentials.get('username').value;
        this.data.password = this.credentials.get('password').value;

        this.loginObserved.next(this.data);
    }

    // User canceled login dialog.  Notify observer.
    public onCancel() {
        this.loginObserved.complete();
    }
}

