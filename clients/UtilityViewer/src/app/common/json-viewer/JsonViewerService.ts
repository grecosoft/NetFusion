import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material';
import { JsonViewerDialog, DialogData } from './dialog/json-viewer.dialog';

// Service that can be injected to display a JS object as JSON representation.
@Injectable()
export class JsonViewerService {

    public constructor(private dialog: MatDialog) {
        
    }

    // Displays a dialog containing JSON and for an array of objects, 
    // allows the user to select an item.  If the user selects an array
    // item, it is returned as result after the dialog is closed.  
    public displayJson(sourceName: string, json: Array<any> | Object | any, 
        showNodeNav: boolean = false): MatDialogRef<JsonViewerDialog> {

        return this.dialog.open(JsonViewerDialog, {
            minWidth: 400,
            minHeight: 250,
            backdropClass: 'my-backdrop',
            panelClass: 'my-panel',
            data: new DialogData(sourceName, json, showNodeNav)
        });
    }

}