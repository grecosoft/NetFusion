import { Component, OnInit, OnDestroy } from '@angular/core';
import { OnNavigatingAway } from '../../../../common/navigation/models';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Application } from '../../models/Application';
import { ApplicationSettings } from '../../models/settings';
import { Subscription } from 'rxjs';

@Component({
    styleUrls: ['./settings.component.scss'],
    templateUrl: './settings.component.html'
})
export class SettingsComponent implements OnInit, OnDestroy, OnNavigatingAway {

    public settingEntry: FormGroup;

    private _entrySubscription: Subscription;

    public constructor(
        private application: Application,
        private formBuilder: FormBuilder) {

    }

    public ngOnInit() {

        this.createEntry(this.application.applicationSettings);
    }

    public ngOnDestroy() {
        if (this._entrySubscription) {
            this._entrySubscription.unsubscribe();
        }
    }

    private createEntry(settings: ApplicationSettings) {

        this.settingEntry = this.formBuilder.group({
            saveOpenedTabs: [settings.saveOpenedTabs],
            monitorForAuthToken: [settings.monitorForAuthToken],
            darkTheme: [settings.darkTheme]
        });

        this._entrySubscription = this.settingEntry.valueChanges
            .subscribe(() => this.updateSettings());
    }
   
    private updateSettings() {

        let settings = Object.assign( 
            this.application.applicationSettings || {},
            this.settingEntry.value);

        this.application.updateSettings(settings);
    }

    public canNavigateAway(): boolean {
        return true;
    }
}


