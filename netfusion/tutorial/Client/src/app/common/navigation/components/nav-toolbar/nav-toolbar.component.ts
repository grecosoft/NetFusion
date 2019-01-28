import { Component, OnInit, Input } from "@angular/core";
import { NavigationService } from "../../services/NavigationService";

@Component({
    selector: 'nav-toolbar',
    templateUrl: 'nav-toolbar.component.html',
    styleUrls: ['nav-toolbar.component.scss']
})
export class NavToolbarComponent implements OnInit {

    @Input()public isDisplayed: boolean;

    public selectedAreaName: string;
    public selectedSectionName: string;

    constructor(
        private navigation: NavigationService) {

    }

    ngOnInit(): void {
        this.selectedAreaName = this.navigation.selectedArea.areaName;
        this.selectedSectionName = this.navigation.selectedMenu.title;
    }

    public get showExtendedToolbar(): boolean {
        return !this.navigation.isSideNavOpened;
    }

    public get selectedMenuIconName(): string {
        return this.navigation.selectedMenu.iconName;
    }
}