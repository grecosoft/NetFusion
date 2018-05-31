import * as _ from 'lodash';
import { Subscription } from 'rxjs';

import { Directive, ViewContainerRef, ComponentFactoryResolver, OnInit, OnDestroy } from '@angular/core';
import { NavigationService } from '../NavigationService';
import { MenuItem } from '../models';
import { Component } from '@angular/compiler/src/core';

// Directive that can be added to an element for displaying the component associated
// with the selected menu item.  NOTE:  for this application the built in routing is
// not being used.  For the workflow of the application, this seemed best.
@Directive({
    selector: '[menu-content]'
})
export class ContentDirective implements OnInit, OnDestroy {
    
    private _menuItemSelected: Subscription;

    constructor(
        private navigation: NavigationService,
        private viewContainerRef: ViewContainerRef,
        private componentFactoryResolver: ComponentFactoryResolver) { 
    }

    // When the selected menu item changes display the component associated
    // with the selected menu item.
    public ngOnInit() {

        // This is the case when the application is initially loaded and the Navigation service determines the 
        // application area and area sub menu to be selected.  This happens before the below subscription to
        // the onMenuItemSelected observable so it needs to be explicitly set.
        if (this.navigation.selectedMenuItem) {
            this.setMenuContentComponent(this.navigation.selectedMenuItem);
        }

        // Monitor for all future user selected area sub menu selections.
        this._menuItemSelected = this.navigation.onMenuItemSelected
            .subscribe((menuItem) => this.setMenuContentComponent(menuItem));
    }

    private setMenuContentComponent(menuItem: MenuItem) {
        let componentFactory = this.componentFactoryResolver.resolveComponentFactory(menuItem.component);

        // Destroy the prior menu-content component and create the component associated with
        // the newly selected menu item.
        this.viewContainerRef.clear();

        // Create the component associated with the selected menu item.
        let menuComponentRef = this.viewContainerRef.createComponent(componentFactory);
        menuItem.componentInstance = menuComponentRef.instance;

        // Pass any navigation data specified by the caller:
        if (menuItem.callerNavData) {
            this.passNavigationData(menuItem, menuItem.callerNavData);
        }
    }

    // Checks of the component being navigate to defines the 'navigatingTo' method.  If so, 
    // it is invoked and passed the provided navigation data.
    private passNavigationData(menuItem: MenuItem, callerNavData: any) {
        
        let prototype = Object.getPrototypeOf(menuItem.componentInstance);

        if (prototype.hasOwnProperty('navigatingTo') && _.isFunction(prototype['navigatingTo']))
        {
            menuItem.componentInstance['navigatingTo'](callerNavData);
        }
    }

    public ngOnDestroy() {
        this._menuItemSelected.unsubscribe();
    }
} 
