import * as _ from 'lodash';

import { Component, Input, OnInit } from '@angular/core';
import { LoadedCompositeApp, PluginViewModel, KnownTypeContractViewModel, KnownTypeDefinitionViewModel, PluginPropValue } from '../../models/composites';
import { PluginSummary, CompositeStructure, PluginModule, KnownTypeDefinition, RegisteredService, PluginDetails } from '../../models/api.models-composite';
import { MatTableDataSource } from '@angular/material';
import { Application } from '../../models/Application';
import { NavigationService } from '../../../../common/navigation/NavigationService';

@Component({
    selector: 'plugin-viewer',
    styleUrls: ['./plugin-viewer.component.scss'],
    templateUrl: './plugin-viewer.component.html'
})
export class PluginViewerComponent implements OnInit {

    @Input('loadedApplication')
    public loadedApplication: LoadedCompositeApp;

    // General property information about the plug-in.
    public pluginProps: PluginPropValue;

    // Plugins - Information about the .net core assemblies identified as
    // being plugins by the server-side application bootstrap process.
    public pluginColumns = ['name', 'type', 'assembly', 'actions'];
    public plugins: MatTableDataSource<PluginViewModel>;

    // Plugin Modules - A plugin can contain one or more modules used to
    // bootstrap its provided services and implementations.
    public moduleColumns = ['name', 'actions'];
    public modules: MatTableDataSource<PluginModule>;
    public selectedModuleLog: {[property: string]:string};

    // Know Type Contracts - These are the abstract base types defined by 
    // the plugin being viewed for which concrete implementations are provided
    // by other plugins.
    public knownTypeContracts: MatTableDataSource<KnownTypeContractViewModel>;
    public knownTypeContractColumns = ['knownTypeName'];

    // Known Type Definitions - These are the concrete types based on known-type
    // contracts used to integrate with the plugin defining the base contract.
    public knownTypeDefinitions: MatTableDataSource<KnownTypeDefinitionViewModel>;
    public knownTypeDefinitionColumns = [ 'definitionTypeName', 'discoveringPlugins'];

    // Type Registrations - These are the services registered by a plug-in components
    // within the dependency-injection container.
    public registrations: MatTableDataSource<RegisteredService>;
    public registrationColumns = ['serviceType', 'registeredType', 'lifeTime'];


    public selectedPlugin: PluginViewModel;

    public constructor(
        private application: Application,
        private navigation: NavigationService) {            

    }

    public ngOnInit() {
        this.navigation.setContentHeaderSegmentText('/', 'Application', 'Plugins');

        this.plugins = new MatTableDataSource(this.createPluginViewModel(this.loadedApplication.structure));
    }

    private createPluginViewModel(structure: CompositeStructure): PluginViewModel[] {

        let pluginToViewModel = (plugins: PluginSummary[], type: string) => 
            _.map(plugins, (p) => {
                return {
                    id: p.id,
                    name: p.name,
                    assembly: p.assembly.split(',')[0],
                    type: type
                };
            });

        return _.concat(
            pluginToViewModel(structure.corePlugins, 'Core'),
            pluginToViewModel(structure.applicationPlugins, 'Application'));
    }

    public viewPluginDetails(pluginViewModel: PluginViewModel) {
        this.application.compositeAppSettings.getPluginDetails(
            this.loadedApplication.compositeApp, pluginViewModel.id).subscribe((details) => {

                this.selectedPlugin = pluginViewModel;

                this.pluginProps = this.GetPluginProperties(details);                            
                this.modules = new MatTableDataSource(details.modules);                        
                this.registrations = new MatTableDataSource(details.registrations);

                this.knownTypeDefinitions =  new MatTableDataSource(
                    this.createDefinitionViewModels(details.knowTypeDefinitions)); 

                let contractTypes: KnownTypeContractViewModel[] = _.map(details.knownTypeContracts, (ktc) => {
                    return {
                        knownTypeName: ktc
                    };
                });

                this.knownTypeContracts = new MatTableDataSource(contractTypes);
            });
    }

    private GetPluginProperties(details: PluginDetails): PluginPropValue {

       return new PluginPropValue(
           details.id,
           details.name,
           details.assembly,
           details.description,
           details.sourceUrl,
           details.docUrl);
    }

    public get isReturnToPluginsEnabled(): boolean {
        return this.selectedPlugin && !this.selectedModuleLog;
    }

    public get isReturnToPluginEnabled(): boolean {
        return this.selectedModuleLog != null;
    }

    private createDefinitionViewModels(definitions: KnownTypeDefinition[]): KnownTypeDefinitionViewModel[] {
        return _.map(definitions, d => {
            return new KnownTypeDefinitionViewModel(d.definitionTypeName, d.discoveringPlugins);
        });
    }

    public viewModuleLog(module: PluginModule) {
        this.selectedModuleLog = module.log;
        console.log(module.log);
    }

    public viewPlugins() {
        this.selectedPlugin = null;
    }

    public viewSelectedPlugin() {
        this.selectedModuleLog = null;
    }

    public get hasModules(): boolean {
        return this.modules.data.length > 0;
    }

    public get hasTypeContracts(): boolean {
        return this.knownTypeContracts.data.length > 0;
    }

    public get hasTypeDefinitions(): boolean {
        return this.knownTypeDefinitions.data.length > 0;
    }

    public get hasRegistrations(): boolean {
        return this.registrations.data.length > 0;
    }
}