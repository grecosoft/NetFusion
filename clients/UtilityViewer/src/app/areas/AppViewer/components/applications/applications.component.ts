import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material';

import { Application} from '../../models/Application';
import { CompositeApp } from '../../models/composites';
import { NavigationService } from '../../../../common/navigation/NavigationService';

@Component({
    styleUrls: ['applications.component.scss'],
    templateUrl: 'applications.component.html',
})
export class ApplicationsComponent implements OnInit {

    public applicationEntry: FormGroup;
    public applicationColumns = ['name', 'address', 'actions'];
    public applications: MatTableDataSource<CompositeApp>;
    public existingApplication: CompositeApp;

    private isAddingNewApp = false;

    public constructor(
        private application: Application,
        private navigation: NavigationService,  
        private formBuilder: FormBuilder) {
    }

    public ngOnInit() {
        this.createEntry();
        this.applications = new MatTableDataSource(this.application.compositeAppSettings.applications);
    }

    public get isEditingApplication(): boolean {
      return !this.application.compositeAppSettings.hasApplications || this.existingApplication != null || this.isAddingNewApp;
    }

    public get displayCancel() {
       return this.isEditingApplication && this.application.compositeAppSettings.hasApplications;
    }

    public get displayClose() {
      return !this.isEditingApplication || !this.application.compositeAppSettings.hasApplications;
    }

    public isInputInvalid(fieldName: string): boolean {
        return this.applicationEntry .get(fieldName).invalid;
    }

    public saveApplication() {
        let formModel = this.applicationEntry.value;

        let compositeApp = Object.assign(this.existingApplication || {}, formModel);

        if (!this.existingApplication) {
            this.application.compositeAppSettings.addApplication(compositeApp);            
        }

       this.resetEntryState();
       this.application.compositeAppSettings.saveApplications();
    }

    public cancelEdit() {
        this.resetEntryState();
    }

    private resetEntryState() {
        this.existingApplication = null;
        this.isAddingNewApp = false;
        this.applicationEntry.reset();    
    }

    public editApplication(compositeApp: CompositeApp) {
        this.existingApplication = compositeApp;
        this.applicationEntry.reset(this.existingApplication);
    }

    public deleteApplication(compositeApp: CompositeApp) {
       this.application.compositeAppSettings.deleteApplication(compositeApp);
       this.applications.data = this.application.compositeAppSettings.applications;
    }

    public addApplication() {
        this.isAddingNewApp = true;
        this.applicationEntry.reset();    
    }

    private createEntry() {

        this.applicationEntry = this.formBuilder.group({
            name: [null, [Validators.required, Validators.minLength(5), Validators.maxLength(20)] ],
            address: [null, [Validators.required, Validators.minLength(5), Validators.maxLength(100)] ],
        });
    }

    

    public navigateToCompositeApp(compositeApp: CompositeApp) {
        this.application.loadCompositeApp(compositeApp);
        this.navigation.navigateToMenuItemById('PLUGINS', compositeApp);
    }
}    
