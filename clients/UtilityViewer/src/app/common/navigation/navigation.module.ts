import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatListModule, MatIconModule, MatToolbarModule } from '@angular/material';

import { PortalComponent } from './components/portal/portal.component';
import { MenuComponent } from './components/menu/menu.component';

import { ContentDirective } from './directives/content.directive';
import { ToolbarDirective } from './directives/toolbar.directive';

import { NavigationService } from './NavigationService';

@NgModule({
    declarations: [
        PortalComponent, 
        MenuComponent, 
        ContentDirective,
        ToolbarDirective
    ],
    imports: [ 
        CommonModule,
        MatListModule,
        MatIconModule,
        MatToolbarModule
    ],
    providers: [NavigationService],
    exports: [ PortalComponent, MenuComponent, ToolbarDirective, ContentDirective]
})
export class NavigationModule {

}