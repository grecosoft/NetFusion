import { ApplicationArea, AreaMenuItem } from "../navigation.models";
import { Injectable, Inject } from "@angular/core";
import { find, flatMap } from "lodash";

@Injectable()
export class MenuDefinitionService {

    private _applicationAreas: ApplicationArea[] = [];
    
    public defaultAreaKey: string;
    public defaultMenuKey: string;

    public defineArea(area: ApplicationArea) {
        this._applicationAreas.push(area);
    }

    public setDefaultAreaMenuKey(areaKey: string, menuKey: string) {
        this.defaultAreaKey = areaKey;
        this.defaultMenuKey = menuKey;
    }

    public get hasDefaultMenu(): boolean {
        return !!this.defaultAreaKey && !!this.defaultMenuKey;
    }

    public get areas(): ApplicationArea[] {
        return this._applicationAreas
    }

    public getAreaByKey(key: string): ApplicationArea {
        return find(this._applicationAreas, {key: key});
    }

    public getAreaMenuByKey(area: ApplicationArea, key: string): AreaMenuItem {
        return find(area.menuItems, {key: key});
    }

    public getAreaMenuOfRoute(route: string) {
        route = route.substring(1, route.length);

        return this.allAreaMenuItems.find(m => m.route.startsWith(route));
    }

    private get allAreaMenuItems(): AreaMenuItem[] {
        return flatMap(this._applicationAreas, a => a.menuItems);
    }
}