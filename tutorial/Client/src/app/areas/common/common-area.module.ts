import { NgModule } from '@angular/core';


import { Routes, RouterModule } from '@angular/router';
import { AreaModule } from '../area.module';
import { JsonPipe } from '@angular/common';
import { ValidationComponent } from './validation.component/validation.component';
import { MappingComponent } from './mapping-component/mapping.component';
import { AttributedEntityComponent } from './attributed-entity.component/attributed-entity.component';
import { AttributedEntityService } from './AttributedEntityService';

const areaRoutes: Routes = [
    { path: 'attrib-entity', component: AttributedEntityComponent },
    { path: 'mapping', component: MappingComponent },
    { path: 'validation', component: ValidationComponent },
];

@NgModule({
    imports: [
        AreaModule,
        RouterModule.forChild(
            areaRoutes
          )
    ],
    declarations: [
        MappingComponent,
        ValidationComponent,
        AttributedEntityComponent
    ],
    entryComponents: [
        MappingComponent,
        ValidationComponent,
        AttributedEntityComponent
    ],
    providers: [
        JsonPipe,
        AttributedEntityService
    ]
})
export class CommonAreaModule {
    
}