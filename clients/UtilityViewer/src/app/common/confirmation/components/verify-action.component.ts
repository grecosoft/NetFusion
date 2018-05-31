import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { ConfirmResponseTypes, ConfirmSettings } from '../models';

@Component({

    styleUrls: ['confirmation.scss'],

    template: `
        <h1 mat-dialog-title class="dialog-title">
            {{data.title}}
        </h1>
        <div mat-dialog-content>
            <div class="dialog-message">
                {{data.message}}  
            </div>
        </div>    
        <div mat-dialog-actions>
            <button mat-button (click)="onConfirmed()" tabindex="2">{{ data.confirmText }}</button>
            <button mat-button (click)="onCancel()" tabindex="-1">{{ data.cancelText }}</button>
        </div>  
    `
})
export class VerifyActionComponent {

    public constructor(
        public dialogRef: MatDialogRef<VerifyActionComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ConfirmSettings) {

    }

    public onConfirmed() {
        this.dialogRef.close(ConfirmResponseTypes.ActionConfirmed);
    }

    public onCancel() 
    {
        this.dialogRef.close(ConfirmResponseTypes.ActionCanceled);
    }
}