import { NgModule } from '@angular/core';

import { ConfirmationService } from './ConfirmationService';
import { VerifyActionComponent } from './components/verify-action.component';

// Angular Modules:
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// UI Modules:
import { FlexLayoutModule } from "@angular/flex-layout";
import {  
    MatButtonModule, 
    MatInputModule, 
    MatDialogModule
} from '@angular/material';

@NgModule({
    providers: [
        ConfirmationService
    ],

    declarations: [
        VerifyActionComponent
    ],

    entryComponents: [
        VerifyActionComponent
    ],
    imports: [
        CommonModule,
        FlexLayoutModule,
        MatButtonModule,
        MatInputModule,
        MatDialogModule
    ]
})
export class ConfirmationModule
{

}