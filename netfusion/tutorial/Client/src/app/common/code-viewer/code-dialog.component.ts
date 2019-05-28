import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';

@Component({
    template: `<div *ngIf="this.codeListing"><pre>{{codeListing}}</pre></div>`
})
export class CodeDialogComponent {

    public codeListing: string;

    constructor(@Inject(MAT_DIALOG_DATA) public data: any) {
        this.codeListing = data.code;
     }
}