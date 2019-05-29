export class ApplicationArea {

    public defaultMenuItemKey: string;
    
    constructor(
        public key: string,
        public areaName: string, 
        public iconName: string,
        public menuItems: AreaMenuItem[]) {

        for (let item of menuItems) {
            item.areaKey = key;
        }
    }
}

export class AreaMenuItem {

    public areaKey: string;

    constructor(
        public key: string,
        public title: string,
        public iconName: string,
        public route: string) {

        }
}

export class NavigationState {

    public static storageKey: string = "common:navigation:state";

    public isSideNavOpened: boolean;
    public selectedAreaKey: string;
    public areaSelections: { [areaKey: string]: AreaState }

    constructor() {
        this.areaSelections = {};
    }
}

export class AreaState {

    constructor(
        public menuKey: string,
        public route: string) {

    }
}

export class SelectedNavigation {

    constructor(
        public appArea: ApplicationArea,
        public menuItem: AreaMenuItem) {

    }
}