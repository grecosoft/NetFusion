import { Component, Input, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
    styleUrls: ['./json-viewer.dialog.css'],
    templateUrl: './json-viewer.dialog.html'
})
export class JsonViewerDialog {

    public constructor(
        public dialogRef: MatDialogRef<JsonViewerDialog>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData) {

         }

    // Called if an array is being displayed from which the
    // user has selected an item.
    public arrayItemSelected(item: any) {
        this.dialogRef.close(item);
    }

}

export class DialogData {

    public constructor(
        public sourceName: string,
        public json: Array<any> | Object | any,
        public showNodeNav: boolean) {
        
        }
}