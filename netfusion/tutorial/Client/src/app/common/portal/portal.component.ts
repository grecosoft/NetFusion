import { Component } from "@angular/core";
import { MenuDefinitionService } from '../navigation/services/MenuDefinitionService';
import { ApplicationArea, SelectedNavigation } from '../navigation/navigation.models';
import { NavigationService } from '../navigation/services/NavigationService';
import { Application } from 'src/app/Application';
import { appSettings } from 'src/app/app.settings';

// The main component of the web application.
@Component({
    selector: 'app-portal',
    templateUrl: './portal.component.html',
    styleUrls: ['./portal.component.scss']
})
export class PortalComponent {
   
    public portal: PortalView;

    public constructor(
        private application: Application,
        menuDefinition: MenuDefinitionService,
        private navigation: NavigationService) {            

        this.portal = new PortalView(menuDefinition.areas);
        this.portal.isLoggedIn = application.isLoggedIn;
        this.portal.isSideNavOpened = navigation.isSideNavOpened;

        navigation.whenNavigationChanged.subscribe(navState => {
            this.portal.selectedArea = navState;
        });

        application.whenLoginStateChange.subscribe((isLoggedIn)=> {
            this.portal.isLoggedIn = isLoggedIn;
        });
    }

    public onSetNavOpenState(value: boolean) {
        this.navigation.setSideNavOpened(value);
        this.portal.isSideNavOpened = this.navigation.isSideNavOpened;
    }

    public onAreaSelected(area: ApplicationArea) {
        this.navigation.setSelectedArea(area);
    }

    public onLogIn() {
        this.application.loginUser();
    }

    public onLogOut() {
        this.application.logOutUser();
    }
}

class PortalView {
    
    public appName: string;
    public selectedArea: SelectedNavigation;
    public isLoggedIn: boolean;
    public isSideNavOpened: boolean;

    constructor(
        public applicationAreas: ApplicationArea[]
    ) {
        this.appName = appSettings.applicationName;
    }
}


