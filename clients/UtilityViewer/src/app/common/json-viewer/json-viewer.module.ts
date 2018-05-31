import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatButtonModule, MatIconRegistry, MatIconModule } from '@angular/material';

import { JsonViewerComponent } from './components/json-viewer.component';
import { JsonViewerDialog } from './dialog/json-viewer.dialog';
import { JsonViewerService } from './JsonViewerService';
import { DomSanitizer } from '@angular/platform-browser';

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

    public constructor(iconRegistry: MatIconRegistry, sanitizer: DomSanitizer) { 
        
        iconRegistry.addSvgIcon('view-node', sanitizer.bypassSecurityTrustResourceUrl(
            'app/common/json-viewer/assets/img/ic_tab_unselected_black_24px.svg'));
     
    }
} 