import * as _ from 'lodash';
import { Type } from '@angular/core';
import { APP_ID } from '../../app.settings';


const APP_AREA = "NAV";

// Stores the last selected ara and section menus used to restore
// last selections when user navigates between areas or reloads
// the application.
export class NavigationConfigSettings {

    public static get storeKey(): string {
        return `${APP_ID}:${APP_AREA}`
    }

    public lastSelectedAreaId: string;
    public lastSelectedSectionId: string;
}

// Used to define a menu associated with a defined area of the application.
export class AreaMenuItem {
    private _featureMenuItems: MenuItem[] = [];
    private _hasAccess: boolean = false;
    private _toolbar: Type<any>;

    public constructor(    
        public id: string,   
        public title: string,
        public iconName: string) {

    }

    // Based on the line assigned areas, determines the menue items to which the
    // accessing user has access.
    public setLoginAccess(areas: {[areaKey: string]:string[]}) {

        const areaFeatures = areas[this.id];
        this._hasAccess = !!areaFeatures;
        
        if (this._hasAccess) {
           _.forEach(this._featureMenuItems, m => m.setLoginAccess(areaFeatures));
        }
    }

    public setAccess(hasAccess: boolean) {
        this._hasAccess = hasAccess;
        _.forEach(this._featureMenuItems, m => m.setAccess(hasAccess));
    }

    public get hasAccess(): boolean {
        return this._hasAccess;
    }

    public get toolbarComponent(): Type<any> {
        return this._toolbar;
    }

    public addFeatureMenu(menuItem: MenuItem): AreaMenuItem {
        this._featureMenuItems.push(menuItem);
        return this;
    }

    public setToolbar(component: Type<any>) {
        this._toolbar = component;
    }

    public get featureMenuItems(): MenuItem[] {
        return this._featureMenuItems;
    }
}

// A menu defined within an application area.
export class MenuItem {
    
    private _hasAccess: boolean = false;

    constructor(
        public id: string,
        public title: string,
        public iconName: string,
        public component: Type<any>) {
    }

    // Properties used by internal implementation.
    public isSelected?: boolean;
    public componentInstance?: any;
    public callerNavData?: any;

    public setLoginAccess(featuresKeys: string[]) {
        this._hasAccess = !!_.find(featuresKeys, fk => fk === this.id)
    }

    public setAccess(hasAccess: boolean) {
        this._hasAccess = hasAccess;
    }

    public get hasAccess(): boolean {
        return this._hasAccess;
    }
}


// Interface that can be implemented by a menu content component
// to specify if navigating to another menu item is allowed.
export interface OnNavigatingAway {

    // Return true to allow navigation.  Otherwise, return false.
    canNavigateAway(): boolean;
}

// Interface that can be implemented by a menu content component
// that is invoked during navigation.  
export interface OnNavigatingTo {

    // If another menu component initiated the navigation and it 
    // specfied data to be passed, the component being navigated 
    // will be passed the data within the callerNavData parameter. 
    navigatingTo(callerNavData?: any);
}

