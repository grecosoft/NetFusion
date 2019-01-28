import { Component } from '@angular/core';
import { SettingsService } from '../SettingsService';
import { CalculationSettings } from '../models';

@Component({
    templateUrl: './settings.component.html',
    styleUrls: ['../../area.scss']
})
export class SettingsComponent {

    public injectedSettings: CalculationSettings;
    public nugetPackageName = "NetFusion.Settings";
    public documentationUrl = "https://github.com/grecosoft/NetFusion/wiki/core.settings.overview#settings---overview";
    public containerScope = "Singleton";

    constructor(
        private settingsService: SettingsService) {

    }

    public loadSettings() {
        this.settingsService.readSettings()
            .subscribe((settings) => {
                this.injectedSettings = settings;
            });
    }

}