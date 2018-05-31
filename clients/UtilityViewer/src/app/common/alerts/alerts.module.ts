import { NgModule } from '@angular/core';
import { AlertService } from './AlertService';

// Angular Modules:
import { CommonModule } from '@angular/common';

// UI Modules:
import { FlexLayoutModule } from "@angular/flex-layout";
import { MatSnackBarModule} from '@angular/material';

@NgModule({
    declarations: [ ],

    providers: [
        AlertService
    ],
    imports: [
        FlexLayoutModule,
        MatSnackBarModule
    ],
    entryComponents: [ ]
})
export class AlertsModule {

}