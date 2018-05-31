// Common Types:
import { NavigationService } from '../../common/navigation/NavigationService';
import { AreaMenuItem, MenuItem } from '../../common/navigation/models';

// Application Area Components:
import { ConnectionsComponent } from './components/connections/connections.component';
import { EntriesComponent } from  './components/entries/entries.component';
import { ResourcesComponent } from './components/resources/resources.component';
import { NotificationsComponent } from './components/notifications/notifications.component';
import { EnvironmentsComponent } from './components/environments/environments.component';
import { SettingsComponent } from  './components/settings/settings.component';
import { AboutComponent } from './components/about/about.component';
import { AreaToolbarComponent } from './components/area-toolbar/area-toolbar.component';

export class HalViewerPortal {
    
    // Called from the area's module when application is being bootstrapped.
    public createPortalMenu(navigation: NavigationService) {

        let areaMenu = this.createAreaMenu(); 

        areaMenu.setToolbar(AreaToolbarComponent);
        areaMenu.setAccess(true);
        
        navigation.addApplicationAreaMenu(areaMenu);
    }

    private createAreaMenu(): AreaMenuItem {

        return new AreaMenuItem('REST-VIEWER', 'REST/Hal Viewer', 'hal-viewer')
            .addFeatureMenu(new MenuItem(
                'CONNECTIONS', 
                'Connections', 
                'nav-connections', 
                ConnectionsComponent))
            .addFeatureMenu(new MenuItem(
                'ENTRIES', 
                'API Entries', 
                'nav-entries', 
                EntriesComponent))
            .addFeatureMenu(new MenuItem(
                'RESOURCES', 
                'Resources', 
                'nav-resources', 
                ResourcesComponent))
            .addFeatureMenu(new MenuItem(
                'NOTIFICATIONS', 
                'Notifications', 
                'nav-notifications', 
                NotificationsComponent))
            .addFeatureMenu(new MenuItem(
                'ENVIRONMENT', 
                'Environments', 
                'nav-environments', 
                EnvironmentsComponent))
            .addFeatureMenu(new MenuItem(
                'SETTINGS', 
                'Settings', 
                'nav-settings', 
                SettingsComponent))
            .addFeatureMenu(new MenuItem(
                'ABOUT', 
                'About', 
                'nav-about', 
                AboutComponent));
    };
}