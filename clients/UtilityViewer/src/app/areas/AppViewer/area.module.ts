// Angular Types:
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

// Common types:
import { NavigationService } from '../../common/navigation/NavigationService';

// Angular Modules:
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// UI Modules:
import { FlexLayoutModule } from "@angular/flex-layout";
import { 
    MatCardModule,
    MatSelectModule, 
    MatTableModule, 
    MatIconModule, 
    MatButtonModule, 
    MatInputModule, 
    MatTabsModule, 
    MatListModule,
    MatTooltipModule,
    MatDialogModule, 
    MatIconRegistry} from '@angular/material';

import { JsonViewerModule } from '../../common/json-viewer/json-viewer.module';

// Area application services:
import { AppViewerPortal } from './AppViewerPortal';
import { Application } from './models/Application';

// Area Components:
import { AboutComponent } from './components/about/about.component';
import { ApplicationsComponent } from './components/applications/applications.component';
import { PluginViewerComponent } from './components/plugin-viewer/plugin-viewer.component';
import { PluginsComponent } from './components/plugins/plugins.component';
@NgModule({
    declarations: [
        AboutComponent,
        ApplicationsComponent,
        PluginViewerComponent,
				PluginsComponent
    ],

    entryComponents: [
        AboutComponent,
        ApplicationsComponent,
        PluginsComponent
    ],

    providers: [ 
        AppViewerPortal,
        Application,
        {
            provide: APP_INITIALIZER,
            useFactory: appFactory,
            deps: [NavigationService, AppViewerPortal, Application],
            multi: true }
    ],

    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,

        FlexLayoutModule,
        MatCardModule,
        MatSelectModule, 
        MatTableModule, 
        MatListModule,
        MatIconModule, 
        MatButtonModule, 
        MatInputModule, 
        MatTabsModule, 
        MatTooltipModule,
        MatDialogModule, 

        JsonViewerModule
    ]
})
export class AppViewerModule {

    constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {  
        iconRegistry.addSvgIcon('app-viewer', sanitizer.bypassSecurityTrustResourceUrl('app/areas/AppViewer/assets/img/ic_view_quilt_white_24px.svg'));
        iconRegistry.addSvgIcon('nav-applications', sanitizer.bypassSecurityTrustResourceUrl('app/areas/AppViewer/assets/img/ic_important_devices_white_24px.svg'));
        iconRegistry.addSvgIcon('nav-plugins', sanitizer.bypassSecurityTrustResourceUrl('app/areas/AppViewer/assets/img/ic_dns_white_24px.svg'));
    }
}

export function appFactory(navigation: NavigationService,
    portal: AppViewerPortal, 
    application: Application ) {

    return () => {

        portal.createPortalMenu(navigation);
        application.restoreState();
      };
   }

