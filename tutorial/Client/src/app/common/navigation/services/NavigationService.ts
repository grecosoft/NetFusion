import { MenuDefinitionService } from './MenuDefinitionService';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from "rxjs/operators";
import { Injectable } from '@angular/core';
import { ApplicationArea, AreaMenuItem, SelectedNavigation, NavigationState, AreaState } from '../navigation.models';
import { LocalStorageService } from '../../services/LocalStorageService';

@Injectable()
export class NavigationService {

    public isSideNavOpened: boolean;

    // Current selections:
    public selectedArea: ApplicationArea;
    public selectedMenu: AreaMenuItem;
   
    // Saved Service State:
    private _navState: NavigationState;

    // Notifications of state change:
    private _navigationUpdated: BehaviorSubject<SelectedNavigation>;
    private navEnded: Observable<NavigationEnd>;
    
    constructor(
        private router: Router,
        private menuDefinition: MenuDefinitionService,
        private localStorage: LocalStorageService
    ) {
        this.initNavigationState();
        this.subscribeNavigationEnded();
    }

    public get whenNavigationChanged(): Observable<SelectedNavigation> {
        return this._navigationUpdated.asObservable();
    }   

    public initNavigationState() {

        // Initialize with configured default values:
        this.isSideNavOpened = true;
        this.selectedArea = this.menuDefinition.getAreaByKey(this.menuDefinition.defaultAreaKey);
        this.selectedMenu = this.menuDefinition.getAreaMenuByKey(this.selectedArea, this.menuDefinition.defaultMenuKey);
        
        // If navigation state present, override defaults with saved values:
        let navState = this.localStorage.load<NavigationState>(NavigationState.storageKey);
        if (navState) {
            let selectedArea = navState.areaSelections[navState.selectedAreaKey];

            this.isSideNavOpened = navState.isSideNavOpened;
            this.selectedArea = this.menuDefinition.getAreaByKey(navState.selectedAreaKey);
            this.selectedMenu = this.menuDefinition.getAreaMenuByKey(this.selectedArea, selectedArea.menuKey);
           
            this._navState = navState;
        }

        // Notify other application components of the initial stated navigation:
        var navSelected = new SelectedNavigation(this.selectedArea, this.selectedMenu);     
        this._navigationUpdated = new BehaviorSubject<SelectedNavigation>(navSelected);
    }

    public setSideNavOpened(value: boolean) {
        this.isSideNavOpened = value;
        this.saveNavigationState();
    }

    public setSelectedArea(area: ApplicationArea) {
        let areaRoute = null;

        let areaState = this._navState && this._navState.areaSelections[area.key];
        if (areaState != null) {
            areaRoute = areaState.route;
        } else {
            areaRoute = area.menuItems[0].route;
        }

        this.router.navigateByUrl(areaRoute);
    }

     // Detect when the user has entered a URL directly into the browser:
     private subscribeNavigationEnded() {

        this.navEnded = this.router.events.pipe (
            filter(evt => evt instanceof NavigationEnd)
            ) as Observable<NavigationEnd>;

        this.navEnded.subscribe((evt: NavigationEnd) => {

            // Determine the menu item based on the URL:
            let menuItem = this.menuDefinition.getAreaMenuOfRoute(evt.url);
            if (menuItem == this.selectedMenu) {
                return;
            }

            if (menuItem != null) {
                 this.selectedMenu = menuItem;
                 this.selectedArea = this.menuDefinition.getAreaByKey(menuItem.areaKey);

                 this._navigationUpdated.next(new SelectedNavigation(this.selectedArea, this.selectedMenu));
            }

            this.saveNavigationState();
        });
    }

    private saveNavigationState() {
        
        if (!this._navState) {
            this._navState = new NavigationState();
        }

        this._navState.isSideNavOpened = this.isSideNavOpened;
        this._navState.selectedAreaKey = this.selectedArea.key;
        this._navState.areaSelections[this.selectedArea.key] = new AreaState(this.selectedMenu.key, this.selectedMenu.route);

        this.localStorage.save(NavigationState.storageKey, this._navState);
    }
}