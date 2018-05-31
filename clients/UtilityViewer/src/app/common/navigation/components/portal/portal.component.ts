import { Subscription} from 'rxjs';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { MenuItem } from '../../models';
import { NavigationService } from '../../NavigationService';

// Application specific component providing a child element with the menu-content
// directive used to display the content associated with the selected menu item.
// The markup for this component wraps the menu's content and provided common UI 
// elements.
@Component({
    selector: 'menu-content',
    styleUrls: ['./portal.component.css'],
    templateUrl: './portal.component.html' 
})
export class PortalComponent implements OnInit, OnDestroy {

    public selectedMenuItem: MenuItem;

    public constructor(
        private navigation: NavigationService) {
        
    }

    private menuItemSelected: Subscription;

    // Subscribe to be called when the selected menu item changes.
    public ngOnInit() {

        this.menuItemSelected = this.navigation.onMenuItemSelected.subscribe(
            (menuItem) => { this.selectedMenuItem = menuItem; });
    }

    public ngOnDestroy() {
        this.menuItemSelected.unsubscribe();
    }

    public get contentMessage(): string {
        return this.navigation.contentInfoMessage;
    }
}