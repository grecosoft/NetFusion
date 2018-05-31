// Library Types:
import * as _ from 'lodash';
import { Observable, Subject} from 'rxjs';

// Angular Types:
import { Injectable, Type } from '@angular/core';
import { MenuItem, AreaMenuItem, NavigationConfigSettings } from './models';
import { SettingsService } from '../settings/SettingsService';

// Service responsible for application area and sub-area navigation.  Each sub-area menu
// is associated with an angular component that is displayed by ContentDirective.
@Injectable()
export class NavigationService {

    private _configSettings : NavigationConfigSettings;

    // Application area menus:
    public areaMenuItems: AreaMenuItem[] = [];
    public selectedAreaMenuItem: AreaMenuItem = null;
    private _areaMenuItemSelected: Subject<AreaMenuItem>;
    private _lastStoredAreaMenuId: string = "REST-VIEWER";

    // Sub application area menus:
    public selectedMenuItem: MenuItem = null;
    private _items: MenuItem[] = [];
    private _menuItemSelected: Subject<MenuItem>;

    public contentInfoMessage: string = '';


    public constructor(
        private settingsService: SettingsService) {

        // Restore the navigation related settings from local storage.
        this._configSettings = settingsService.read<NavigationConfigSettings>(
            NavigationConfigSettings.storeKey);

        this._configSettings = this._configSettings || new NavigationConfigSettings();

        // Define a subject that can be observed by other application services
        // and components when the selected menu changes.
        this._areaMenuItemSelected = new Subject<AreaMenuItem>();
        this._menuItemSelected = new Subject<MenuItem>();
    }

    // Called during the bootstrap of the application to allow each application area
    // to define the sub-areas and the component to be displayed,
    public addApplicationAreaMenu(areaMenuItem: AreaMenuItem) {
        this.areaMenuItems.push(areaMenuItem);
    }

    // Called when the main component of the application is initialized to restore
    // the last selected area and sub-ara.
    public selectInitialArea() {

        // Default to the first area menu item.
        this.selectedAreaMenuItem = this.areaMenuItems.length > 0 ? this.areaMenuItems[0] : null;
        this._areaMenuItemSelected.next(this.selectedAreaMenuItem);

        // Check if the stored id of the last selected menu item exists.
        let menuIdx = _.findIndex(this.areaMenuItems, {id: this._configSettings.lastSelectedAreaId });
        if (menuIdx > -1) {
            this.selectedAreaMenuItem = this.areaMenuItems[menuIdx];
        }

        // Set the sub-area menu items for the initial area.
        if (this.selectedAreaMenuItem != null) {
            this.setMenuItems(this.selectedAreaMenuItem.featureMenuItems);
        }
    }

    // Should be called when the use takes an action to navigate between available application area.
    public setSelectedArea(areaMenuItem: AreaMenuItem) {
        this.selectedAreaMenuItem = areaMenuItem;
        this._areaMenuItemSelected.next(this.selectedAreaMenuItem);
        this.setMenuItems(areaMenuItem.featureMenuItems);

        this._configSettings.lastSelectedAreaId = areaMenuItem.id;
        this.saveConfigSettings();
    }
    
    // Defines the main navigation menu used to navigate the application's supported features.
    // This method should be invoked when the application is bootstrapped if the application
    // has only one application area.  If the application has multiple areas, then this method
    // should be invoked with an area's menu items when the selected area changes.
    public setMenuItems(menuItems: MenuItem[]) {
       
        this.setMenuItemDefaults(menuItems);

        let menuIdx = _.findIndex(menuItems, { id: this._configSettings.lastSelectedSectionId });
        if (menuIdx > -1) {
            this.selectedMenuItem = menuItems[menuIdx];
        } else {
            this.selectedMenuItem = (menuItems.length > 0 && menuItems[0]) || null;
        }

        this._items = menuItems;   
        this.selectedMenuItem.isSelected = true;
        
        
        // Notify subscribers that the selected menu item has changed.
        this._menuItemSelected.next(this.selectedMenuItem);
    }

    // The menu items for an application with a single application area or the 
    // menu items for the currently selected area of a multi-area application. 
    public get menuItems(): MenuItem[] {
        return this._items;
    }

    public getMenuItem(id: string): MenuItem {
        return _.find(this._items, item => item.id === id);
    }

    // Updates the selected menu item and notifies all observers.
    public navigateToMenuItem(menuItem: MenuItem) {

        if (menuItem == this.selectedMenuItem) {
            return;
        }

        // Delegate to the component, of the currently selected menu, to determine
        // if navigation away from the currently selected menu is allowed.
        if (this.selectedMenuItem && !this.canLeaveCurrentView(this.selectedMenuItem))
        {
            return;
        }

        // Prior Menu Item:
        this.selectedMenuItem.componentInstance = null;
        this.selectedMenuItem.isSelected = false;
        this.selectedMenuItem.callerNavData = null;

        // Updated the current selected Menu Item:
        this.selectedMenuItem = menuItem;
        this.selectedMenuItem.isSelected = true;

        // Notify observers of the new menu item selection.
        this._menuItemSelected.next(menuItem);
        this._configSettings.lastSelectedSectionId = menuItem.id;
    }

    // Can be used to navigate to a new menu based on user action.
    public navigateToMenuItemById(id: string, navigationData?: any) {
        let menuItem = this.getMenuItem(id);

        if (menuItem == null) {
            throw new Error("The menu with the ID of " + id + " is not valid");
        }

        menuItem.callerNavData = navigationData;
        this.navigateToMenuItem(menuItem);
    }

    public setContentHeaderSegmentText(separator: string, ...segments: string[]) {
        this.contentInfoMessage = segments.join(separator);   
    }

    public get onAreaMenuItemSelected(): Observable<AreaMenuItem> {
        return this._areaMenuItemSelected.asObservable();
    }

    // An observable to which services and components can subscribe to be notified
    // when the selected menu has been updated.
    public get onMenuItemSelected(): Observable<MenuItem> {
        return this._menuItemSelected.asObservable();
    }

    private saveConfigSettings() {
        this.settingsService.save(NavigationConfigSettings.storeKey, this._configSettings);
    }

    private setMenuItemDefaults(menuItems: MenuItem[]) {
        _.forEach(menuItems, (menuItem) => {
            menuItem.isSelected = false;
            menuItem.componentInstance = null;
            menuItem.callerNavData = null;
        });
    }

    // Determines if the component associated with the menu item contains a method named canNavigateAway.
    // If so it invokes the method to determine if the user is allowed to navigate to the newly selected menu.
    private canLeaveCurrentView(menuItem: MenuItem): boolean {
        let prototype = Object.getPrototypeOf(menuItem.componentInstance);

        if (prototype.hasOwnProperty('canNavigateAway') && _.isFunction(prototype['canNavigateAway']))
        {
            let canNavigate = menuItem.componentInstance['canNavigateAway']();
            return _.isBoolean(canNavigate) ? canNavigate : false;
        }

        return true;
    }
}