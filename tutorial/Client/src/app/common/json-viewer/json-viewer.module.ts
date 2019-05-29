import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatButtonModule, MatIconModule } from '@angular/material';
import { JsonViewerService } from './services/JsonViewerService';
import { JsonViewerComponent } from './components/json-viewer/json-viewer.component';
import { JsonViewerDialog } from './components/viewer-dialog/json-viewer.dialog';


@NgModule({
    providers: [ JsonViewerService ],
    declarations: [JsonViewerComponent, JsonViewerDialog],
    imports: [ 
        CommonModule,
        MatDialogModule,
        MatIconModule,
        MatButtonModule
    ],
    exports: [ JsonViewerComponent, JsonViewerDialog],
    entryComponents: [JsonViewerDialog]
})
export class JsonViewerModule {

} 