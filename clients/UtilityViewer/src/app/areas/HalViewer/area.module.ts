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
    MatListModule,    
    MatTabsModule, 
    MatTooltipModule,
    MatDialogModule, 
    MatMenuModule,
    MatButtonToggleModule,
    MatSlideToggleModule,
    MatIconRegistry} from '@angular/material';

import { JsonViewerModule } from '../../common/json-viewer/json-viewer.module';

// Area application services:
import { HalViewerPortal } from './HalViewerPortal';
import { Application } from './models/Application';

// Area Components:
import { AboutComponent } from './components/about/about.component';
import { AreaToolbarComponent } from './components/area-toolbar/area-toolbar.component';
import { ConnectionsComponent } from './components/connections/connections.component';
import { EmbeddedViewerComponent } from './components/embedded-viewer/embedded-viewer.component';
import { EntriesComponent } from './components/entries/entries.component'
import { EnvironmentsComponent } from './components/environments/environments.component';
import { MethodIndicatorComponent } from './components/method-indicator/method-indicator.component';
import { NotificationViewerComponent } from './components/notification-viewer/notification-viewer.component';
import { NotificationsComponent } from './components/notifications/notifications.component';
import { RelationViewerComponent } from './components/relation-viewer/relation-viewer.component';
import { ResourceViewerComponent } from './components/resource-viewer/resource-viewer.component';
import { ResourcesComponent } from './components/resources/resources.component';
import { SettingsComponent } from './components/settings/settings.component';

// The main module for the application area:
@NgModule({
    declarations: [
        AboutComponent,
        AreaToolbarComponent,
        ConnectionsComponent,
        EmbeddedViewerComponent,
        EntriesComponent,
        EnvironmentsComponent,
        MethodIndicatorComponent,
        NotificationViewerComponent,
        NotificationsComponent,
        RelationViewerComponent,
        ResourceViewerComponent,
        ResourcesComponent,
        SettingsComponent
    ],

    entryComponents: [
        AboutComponent,
        AreaToolbarComponent,
        ConnectionsComponent,
        EntriesComponent,
        EnvironmentsComponent,
        NotificationsComponent,
        ResourcesComponent,
        SettingsComponent
    ],

    providers: [ 
        HalViewerPortal,
        Application,
        {
            provide: APP_INITIALIZER,
            useFactory: appFactory,
            deps: [NavigationService, HalViewerPortal, Application],
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
        MatIconModule, 
        MatButtonModule, 
        MatInputModule, 
        MatListModule,
        MatTabsModule, 
        MatTooltipModule,
        MatDialogModule, 
        MatMenuModule,
        MatButtonToggleModule,
        MatSlideToggleModule,

        JsonViewerModule
    ]
})
export class HalViewerModule {

    constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {  

        iconRegistry.addSvgIcon('hal-viewer', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_beenhere_white_24px.svg'));
        iconRegistry.addSvgIcon('nav-notifications', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_alarm_white_24px.svg'));   
        iconRegistry.addSvgIcon('nav-settings', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_settings_white_24px.svg'));   
        iconRegistry.addSvgIcon('nav-connections', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_location_on_white_24px.svg')); 
        iconRegistry.addSvgIcon('nav-environments', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_language_white_24px.svg'));   
        iconRegistry.addSvgIcon('nav-entries', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_image_aspect_ratio_white_24px.svg'));   
        iconRegistry.addSvgIcon('nav-resources', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_group_work_white_24px.svg'));  
        iconRegistry.addSvgIcon('view-properties', sanitizer.bypassSecurityTrustResourceUrl('app/areas/HalViewer/assets/img/ic_list_white_24px.svg'));
    }
}

// Executed during application bootstrap to initialize the area's menu structure
// and to restore the state of the area's application.
export function appFactory(navigation: NavigationService, 
    portal: HalViewerPortal, 
    application: Application) {
    
        return () => {

        portal.createPortalMenu(navigation);
        application.restoreState();
      };
   }