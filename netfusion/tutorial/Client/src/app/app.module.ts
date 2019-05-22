// Angular:
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { Routes, RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { StorageServiceModule } from 'angular-webstorage-service';

// Modules:
import { MaterialModule } from './material.module';

// Root Application Services (Singletons)
import { Application } from './Application';
import { LocalStorageService } from './common/services/LocalStorageService';
import { MenuDefinitionService } from './common/navigation/services/MenuDefinitionService';
import { NavigationService } from './common/navigation/services/NavigationService';

// Components:
import { PortalComponent } from './common/portal/portal.component';

// Types:
import { Portal } from './Portal';
import { NotificationService } from './common/services/NotificationService';
import { DialogService } from './common/dialogs/DialogService';
import { CommonAppModule } from './common/common.app.module';
import { ServiceApi } from './api/ServiceApi';
import { TutorialInfoComponent } from './overview/tutorial-info/tutorial-info.component';



// Routes for the main application areas:
const areaRoutes: Routes = [
  { path: 'areas/integration', loadChildren: 'src/app/areas/integration/integration.module#IntegrationModule' },
  
  // { path: 'areas/core', loadChildren: 'src/app/areas/examples.module#ExamplesModule' },

   { path: 'overview/tutorial-info', component: TutorialInfoComponent },
];

// Bootstraps the application:
export function bootstrap_app(application: Application, 
  menuDefinition: MenuDefinitionService) {
  
    return () => Portal.bootstrap(application, menuDefinition).toPromise();
}

// The main application module.  Note:  any registered service providers are 
// considered singleton services and shared across all application sub modules.
@NgModule({
  declarations: [
    PortalComponent,
    TutorialInfoComponent
  ],
  imports: [
    HttpClientModule,
    BrowserAnimationsModule,   
    StorageServiceModule,  

    RouterModule.forRoot(
      areaRoutes,
      { enableTracing: false } 
    ),

    CommonAppModule,
    MaterialModule
  ],
  entryComponents: [
    TutorialInfoComponent,
  ],
  providers: [
    Application,
    LocalStorageService,
    MenuDefinitionService,
    NavigationService,
    NotificationService,
    DialogService,
    ServiceApi,
    { 
      provide: APP_INITIALIZER, useFactory: bootstrap_app, multi: true, 
      deps: [
        Application,
        MenuDefinitionService]
    }
  ],
  bootstrap: [PortalComponent]
})
export class AppModule { }
