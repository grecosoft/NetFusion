import { MatDialogRef, MatDialog, MatDialogConfig } from "@angular/material";
import { Injectable } from "@angular/core";
import { ComponentType } from "@angular/cdk/overlay/index";
import { ConfirmationInfo } from "../services/services.models";
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';

@Injectable()
export class DialogService {

    constructor(
        private dialog: MatDialog) {

    }

    // Delegates to the Angular Material dialog service after
    // setting common properties.
    public openDialog<TDialog>(component: ComponentType<TDialog>, 
        data: any, 
        config: MatDialogConfig): MatDialogRef<TDialog> {

        config.disableClose = false;
        config.panelClass = config.panelClass || 'portal-dialog';
        config.data = data

        return this.dialog.open(component, config);
    }

    public confirmAction(dialogInfo: ConfirmationInfo): MatDialogRef<ConfirmDialogComponent> {

        var dialogComponent: ComponentType<ConfirmDialogComponent> = ConfirmDialogComponent;

        return this.openDialog(dialogComponent, dialogInfo, 
            { width: '300px', minWidth: '300px', height: '150px', minHeight: '150px'});
    }
}