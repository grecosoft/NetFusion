import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { 
    MatSidenavModule,
    MatIconModule, 
    MatSelectModule,
    MatListModule, 
    MatToolbarModule, 
    MatDialogModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatButtonModule } from '@angular/material';

// Components:
import { LoginDialogComponent } from './dialogs/login-dialog/login-dialog.component';
import { NavMenuComponent } from './navigation/components/nav-menu/nav-menu.component';
import { NavToolbarComponent } from './navigation/components/nav-toolbar/nav-toolbar.component';
import { ConfirmDialogComponent } from './dialogs/confirm-dialog/confirm-dialog.component';
import { CodeDialogComponent } from '../areas/github/code-viewer/code-dialog.component';


// Module importing modules on which common application components are dependent.
@NgModule({
    declarations: [
        NavMenuComponent,
        NavToolbarComponent,
        LoginDialogComponent,
        CodeDialogComponent,
        ConfirmDialogComponent
    ],
    imports: [
        CommonModule,
        RouterModule,
        MatIconModule,
        MatListModule,
        MatSidenavModule,
        MatSelectModule,
        MatFormFieldModule,
        MatInputModule,
        MatToolbarModule,
        MatDialogModule,
        FormsModule,
        MatButtonModule,
        ReactiveFormsModule
    ],
    exports: [
        NavMenuComponent,
        NavToolbarComponent,
        LoginDialogComponent,
        MatSidenavModule
    ],
    entryComponents: [
        LoginDialogComponent,
        CodeDialogComponent,
        ConfirmDialogComponent
    ]
})
export class CommonAppModule {

}