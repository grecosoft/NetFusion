import { NgModule } from '@angular/core';

import { SettingsService } from './SettingsService';

// Contains services for persisting application settings.
@NgModule({
    providers: [
        SettingsService
    ]
})
export class SettingsModule {

}