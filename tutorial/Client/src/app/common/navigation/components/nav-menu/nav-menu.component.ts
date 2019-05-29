import { Component, Input, ChangeDetectionStrategy, Output, EventEmitter } from "@angular/core";
import { AreaMenuItem, ApplicationArea, SelectedNavigation } from "../../navigation.models";

@Component({
    selector: 'nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavMenuComponent {

    @Input()public applicationAreas: ApplicationArea[];
    @Input()public selected: SelectedNavigation;
    @Output()public areaChanged = new EventEmitter<ApplicationArea>();

    public isSelectedMenu(menuItem: AreaMenuItem): boolean {
        return menuItem == this.selected.menuItem;
    }

    public isMenuEnabled(menuItem: AreaMenuItem): boolean {
        return true;
    }

    public isBinding() {
        console.log("bind");
    }

    public onUpdate() {}

    public onAreaSelected(area: ApplicationArea) {
        this.areaChanged.next(area);
    }
}