import { NgModule } from '@angular/core';

import { Routes, RouterModule } from '@angular/router';
import { AreaModule } from '../area.module';
import { JsonPipe } from '@angular/common';
import { TemplateUrlsComponent } from './rest/templateUrls/templateUrls.component';
import { ResourceLinkingComponent } from './rest/resourceLinking/resource-linking.component';
import { RestService } from './rest/RestService';

const areaRoutes: Routes = [
    { path: 'rest/resource-linking', component: ResourceLinkingComponent },
    { path: 'rest/template-urls', component: TemplateUrlsComponent },
];

@NgModule({
    imports: [
        AreaModule,
        RouterModule.forChild(
            areaRoutes
          )
    ],
    declarations: [
        ResourceLinkingComponent,
        TemplateUrlsComponent
    ],
    entryComponents: [
        ResourceLinkingComponent,
        TemplateUrlsComponent
    ],
    providers: [
        JsonPipe,
        RestService
    ]
})
export class WebAreaModule {
    
}