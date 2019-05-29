import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ConfirmationInfo } from '../../services/services.models';

@Component({
    templateUrl: 'confirm-dialog.component.html',
    styleUrls: ['confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {

    constructor(
        private dialogRef: MatDialogRef<ConfirmDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public confirmInfo: ConfirmationInfo) { 
            
    }

    public onActionConfirmed() {
        this.dialogRef.close(true);
    }

    public onCancelAction() {
        this.dialogRef.close(false);
    }
}

