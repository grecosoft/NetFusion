import { NgModule } from '@angular/core';


import { Routes, RouterModule } from '@angular/router';
import { AreaModule } from '../area.module';
import { JsonPipe } from '@angular/common';
import { ValidationComponent } from './validation.component/validation.component';
import { MappingComponent } from './mapping-component/mapping.component';
import { EntityExpressionsComponent } from './entity-expressions.component/entity-expressions.component';
import { AttributedEntityComponent } from './attributed-entity.component/attributed-entity.component';

const areaRoutes: Routes = [
    { path: 'attrib-entity', component: AttributedEntityComponent },
    { path: 'entity-exp', component: EntityExpressionsComponent },
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
        AttributedEntityComponent,
        EntityExpressionsComponent
    ],
    entryComponents: [
        MappingComponent,
        ValidationComponent,
        AttributedEntityComponent,
        EntityExpressionsComponent
    ],
    providers: [
        JsonPipe
    ]
})
export class CommonAreaModule {
    
}