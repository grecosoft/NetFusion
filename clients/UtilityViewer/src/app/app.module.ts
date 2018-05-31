
// Type References:
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MatIconRegistry } from '@angular/material';

// Angular Modules:
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

// Common Modules:
import { NavigationModule } from './common/navigation/navigation.module';
import { SettingsModule } from './common/settings/settings.module';
import { ClientModule } from './common/client/client.module';
import { PersistenceModule } from 'angular-persistence';
import { ConfirmationModule } from './common/confirmation/confirmation.module';
import { AlertsModule } from './common/alerts/alerts.module';

// Angular Material/Layout Modules:
import { FlexLayoutModule } from "@angular/flex-layout";
import { 
  MatToolbarModule,
  MatSidenavModule, 
  MatButtonModule,
  MatSelectModule, 
  MatIconModule } from '@angular/material';

// Application Area Modules:
import { AppViewerModule } from './areas/AppViewer/area.module';
import { HalViewerModule } from './areas/HalViewer/area.module';

// Main Application Components:
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,

    NavigationModule,
    SettingsModule,
    ClientModule,
    PersistenceModule,
    ConfirmationModule,
    AlertsModule,

    FlexLayoutModule,
    MatToolbarModule,
    MatSidenavModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,

    AppViewerModule,
    HalViewerModule
    
  ],
  bootstrap: [AppComponent],
})
export class AppModule {

  constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) {  
    iconRegistry.addSvgIcon('side-menu', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_menu_white_24px.svg'));  
    iconRegistry.addSvgIcon('app-icon', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_blur_on_white_24px.svg'));   
    iconRegistry.addSvgIcon('app-areas', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_view_module_white_24px.svg'));   
        
    iconRegistry.addSvgIcon('nav-about', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_help_outline_white_24px.svg'));
    iconRegistry.addSvgIcon('nav-close', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_close_white_24px.svg'));  

    iconRegistry.addSvgIcon('open-dialog', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_open_in_new_white_24px.svg'));
    iconRegistry.addSvgIcon('open-tab', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_tab_white_24px.svg'));

    iconRegistry.addSvgIcon('add-item', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_playlist_add_white_24px.svg'));   
    iconRegistry.addSvgIcon('edit-item', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_edit_white_24px.svg'));
    iconRegistry.addSvgIcon('delete-item', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_delete_forever_white_24px.svg'));    
    iconRegistry.addSvgIcon('process-item', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_playlist_add_check_white_24px.svg'));   

    iconRegistry.addSvgIcon('input-text', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_text_format_white_24px.svg'));
    iconRegistry.addSvgIcon('input-url', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_edit_location_white_24px.svg'));
    iconRegistry.addSvgIcon('input-entry', sanitizer.bypassSecurityTrustResourceUrl('assets/img/ic_device_hub_white_24px.svg'));
  }
 }
 



 