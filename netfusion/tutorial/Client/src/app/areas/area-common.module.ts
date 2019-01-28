import { NgModule } from '@angular/core';
import { MaterialModule } from '../material.module';
import { FlexLayoutModule } from '@angular/flex-layout';
import { CommonAppModule } from '../common/common.app.module';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { JsonViewerModule } from '../common/json-viewer/json-viewer.module';

const reExportedModules = [
    MaterialModule,
    FlexLayoutModule,
    CommonModule,
    CommonAppModule,
    FormsModule,
    ReactiveFormsModule,
    JsonViewerModule
];

// Module containing default set of re-exports that can be 
// included by aan area's specific module.
@NgModule({
    imports: reExportedModules,
    exports: reExportedModules,
    providers: [
       
    ]
})
export class AreaCommonModule {

}