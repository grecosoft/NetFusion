import * as _ from 'lodash';

import { Component } from '@angular/core';
import { AreaMenuItem } from './common/navigation/models';
import { NavigationService } from './common/navigation/NavigationService';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
   

  public constructor(
    private navigation: NavigationService) {
  }


  public ngOnInit() {
    this.navigation.selectInitialArea();
  }

  public get selectedAreaMenuItem(): AreaMenuItem {
    return this.navigation.selectedAreaMenuItem;
  }

  public areaMenuItemSelected(areaMenu: AreaMenuItem) {

    this.navigation.setSelectedArea(areaMenu);
  }

  public get areaMenuItems(): AreaMenuItem[] {
    return _.filter(this.navigation.areaMenuItems, m => m.hasAccess);
  }

  public get hasAreaToolbar(): boolean {
    return this.navigation.selectedAreaMenuItem.toolbarComponent != null;
  }
}


