import { Directive, OnInit, OnDestroy, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { NavigationService } from '../NavigationService';
import { Subscription } from 'rxjs';
import { AreaMenuItem } from '../models';

@Directive({
    selector: '[area-toolbar]'
})
export class ToolbarDirective implements OnInit, OnDestroy {

    private _menuItemSelected: Subscription;

    constructor(
        private navigation: NavigationService,
        private viewContainerRef: ViewContainerRef,
        private componentFactoryResolver: ComponentFactoryResolver) { 
    }
    
    public ngOnInit() {

        // When the directive first initializes, check if the selected area menu
        // has an associated custom toolbar.
        let areaMenu = this.navigation.selectedAreaMenuItem;

        if (areaMenu && areaMenu.toolbarComponent) {
            this.setAreaToolbarContentComponent(areaMenu);
        }

        // Monitor for all future user selected area menu selections.
        this._menuItemSelected = this.navigation.onAreaMenuItemSelected
            .subscribe((areaMenuItem) => this.setAreaToolbarContentComponent(areaMenuItem));
    }

    private setAreaToolbarContentComponent(areaMenuItem: AreaMenuItem) {

        // Destroy the prior area toolbar component and create the component associated with
        // the newly selected menu item.
        this.viewContainerRef.clear();

        if (!areaMenuItem.toolbarComponent) {
            return;
        }

        let componentFactory = this.componentFactoryResolver.resolveComponentFactory(areaMenuItem.toolbarComponent);

        // Create the toolbar component associated with the selected area menu item.
        this.viewContainerRef.createComponent(componentFactory);
    }

    public ngOnDestroy() {
        this._menuItemSelected.unsubscribe();
    }
}