import * as _ from 'lodash';
import { Component } from '@angular/core';

import { MenuItem } from '../../models';
import { NavigationService } from '../../NavigationService';

// Menu component displaying the main navigation menu assocated with the application.
@Component({
    selector: 'main-menu',
    templateUrl: './menu.component.html',
    styleUrls: ['./menu.component.css']
})
export class MenuComponent {

    public constructor(
        private navigation: NavigationService) {

    }

    // The defined main application menu items.
    public get menuItems(): MenuItem[] {
        return _.filter(this.navigation.menuItems, m => m.hasAccess);
    }
     
    // Invoked when the user selects a menu item and delegates to
    // the navigation service.
    public menuItemSelected(menuItem: MenuItem) {

        this.navigation.navigateToMenuItem(menuItem);
    }
}

