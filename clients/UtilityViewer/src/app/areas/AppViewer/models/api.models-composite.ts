
export class CompositeStructure {
    public hostPluginAssembly: string;
    public appPluginAssemblies: string[];
    public corePluginAssemblies: string[];

    public hostPlugin: PluginSummary;
    public applicationPlugins: PluginSummary[];
    public corePlugins: PluginSummary[];
}

export class PluginSummary {
    public id: string;
    public name: string;
    public assembly: string;
}

export class PluginDetails {
    public id: string;
    public name: string;
    public assembly: string;
    public description: string;
    public sourceUrl: string;
    public docUrl: string;

    public knownTypeContracts: string[];
    public knowTypeDefinitions: KnownTypeDefinition[]; // TODO:  Fix spelling on server
    public modules: PluginModule[];
    public registrations: RegisteredService[];
}

export class KnownTypeDefinition {
    public definitionTypeName: string;
    public discoveringPlugins: string[];
}

export class PluginModule {
    public name: string;
    public log: {[property: string]:string};
}

export class RegisteredService {
    public registeredType: string;
    public serviceType: string;
    public lifeTime: string;
}