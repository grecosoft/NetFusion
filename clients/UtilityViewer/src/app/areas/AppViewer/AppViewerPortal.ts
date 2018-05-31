// Common Types:
import { NavigationService } from '../../common/navigation/NavigationService';
import { AreaMenuItem, MenuItem } from '../../common/navigation/models';
import { AboutComponent } from './components/about/about.component';
import { ApplicationsComponent } from './components/applications/applications.component';
import { PluginsComponent } from './components/plugins/plugins.component';

// Application Area Components:

export class AppViewerPortal {

    // Called from the area's module when application is being bootstrapped.
    public createPortalMenu(navigation: NavigationService) {

        let areaMenu = this.createAreaMenu();
        areaMenu.setAccess(true);
        navigation.addApplicationAreaMenu(areaMenu);
    }

    private createAreaMenu(): AreaMenuItem {

        return new AreaMenuItem('APP-VIEWER', 'Composite App Viewer', 'app-viewer')
            .addFeatureMenu(new MenuItem(
                'APPLICATIONS', 
                'Applications', 
                'nav-applications', 
                ApplicationsComponent))    

            .addFeatureMenu(new MenuItem(
                'PLUGINS', 
                'Plug-Ins', 
                'nav-plugins', 
                PluginsComponent))    

            .addFeatureMenu(new MenuItem(
                'ABOUT', 
                'About', 
                'nav-about', 
                AboutComponent));
    };
}