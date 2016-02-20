# Bootstrap Overview
>The bootstrap process is dependent on the following technologies:  MEF and Autofac.  The bootstrap process uses MEF to discover the assemblies and classes used to initialize the application container.  The end result of the bootstrap process is an initialized Autofac dependency injection container.  The design is based on the following:

## Composite Manifest
Identifies the application assemblies that should be included in the bootstrap process.  There are three types of composite manifests:

* ___Host___
: The assembly that is the execution host of the application.  This can be any of the common .NET application types.

* ___Application___
: Application specific assemblies.  This can be assemblies containing an application’s implementation specific artifacts such as database access.  Also included are an application’s business domain specific assemblies.  

* ___Core___
: Assemblies containing reusable technology centric plug-ins.

### Known Type
A types that is defined by a given plug-in but whose implementations are discovered in assemblies participating in the bootstrap process.

### Discovered Type
A type contained within a participating assembly that is based on a Known Type of another plug-in assembly.

### Plug-in Module
A class containing code within a plug-in used to integrate with the bootstrap process and called at specific steps during initialization.

>MEF is used to discover the participating assemblies containing the specific Composite manifests and other types, such as the Plug-in Modules, that are used during application initialization.

# Setup
The minimal bootstrap configuration follows:

1. Add the following infrastructure Nuget packages to the host project:

	* NetFusion.Bootstrap
	* NetFusion.Settings

2. Add the following to the host’s configuration file:

	``` xml
	<?xml version="1.0" encoding="utf-8"?>
	<configuration>
	  <configSections>
	    <section name="netFusion" type="NetFusion.Configuration.Host.NetFusionSection, NetFusion.Configuration" />
	  </configSections>

	  <netFusion>
	    <hostConfig environment = "Development" />

	    <mongoAppSettingsConfig
	      mongoUrl = "mongodb://localhost:27017"
	      databaseName = "NetFusion"
	      collectionName = "NetFusion.AppSettings" />

	  </netFusion>
	</configuration>
	```

3. Define the application manifest:

	``` csharp
using NetFusion.Bootstrap.Manifests;

namespace TestWebHost
{
    public class Manifest : IAppHostPluginManifest
    {
        public string PluginId => "55b93b110a9343584430ff21b";
        public string Name => "Example Web Host";
        public string Description => "Application host that exposes an external REST interface.";
        public string AssemblyName => this.GetType().Assembly.FullName;
    }
}
```
4. The following is the minimal bootstrap configuration that should take place at the start of the application host.  A WebApi application is being used for this demonstration.

	``` csharp
	public class WebApiApplication : System.Web.HttpApplication
	   {
	       protected void Application_Start()
	       {
	          var netFusionConfig =  (NetFusionSection)ConfigurationManager.GetSection("netFusion");

	           AppContainer.Create(AppDomain.CurrentDomain.RelativeSearchPath)
	               .WithConfig(netFusionConfig)
	               .Load()
	               .Start();
	       }
	   }
	```

# Logging
>The host application can specify a logger to be used.  During the bootstrap process, the AppContainer will log any exceptions before they are raised to the caller.  If a logger is not specified, then the calling application host is responsible for logging exceptions that are raised.  The application host can configure its logger of choice.

The following shows how the container can be configured to use Serilog.

1. Add the following infrastructure NuGet packages to the host project:

	* NetFusion.Logging.Serilog

2. Logging is added to the ***AppContainer*** as follows:

	* Create the Serilog logger.
	* Add the ***LoggerConfig*** container configuration and specify the logger.
	* Add the ***AutofacRegistrationConfg*** container configuration and register the logger.

	After logging has been configured, the bootstrap code should looks as follows:

	``` csharp
	public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
           var netFusionConfig =  (NetFusionSection)ConfigurationManager.GetSection("netFusion");

            // Create logger:
            var logConfig = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341");

            logConfig.MinimumLevel.Debug();
            var logger = logConfig.CreateLogger();

            // Configuration Application Container:
            AppContainer.Create(AppDomain.CurrentDomain.RelativeSearchPath)
                .WithConfig(netFusionConfig)

                .WithConfig((LoggerConfig config) =>
                {
                    config.LogExceptions = true;
                    config.SetLogger(new PluginLogger(logger));
                })

                .WithConfig((AutofacRegistrationConfig config) =>
                {
                    config.Build = builder =>
                    {
                        // Add any components created during startup.
                        builder.RegisterLogger(logger);
                    };
                })
                .Load()
                .Start();
        }
    }
	```

The above adds the ***LoggerConfig*** as a container configuration and the LogExceptions property is specified.  If set to True, the ***AppContainer*** will log all exceptions before they are raised.  Next, an instance of the ***SerilogPluginLogger*** is created and passed to the SetLogger method.  The instance of this class is provided by the Netfusion.Logging.Serilog assembly and provides an implementation of ***IContainerLogger*** by delegating to the passed Serilog logger.

Then the ***AutofacRegistrationConfig*** is specified.  This adds the Serilog logger to the Autofac container.  This allows the Serilog ***ILogger*** to be injected into application components that require logging.

# Configuration

>A plugin may need configurations specified by the host application for certain settings.  Plugins define configurations by deriving a class from the ***IContainerConfig*** marker interface.  

The following is a configuration from the WebApi plugin:

``` csharp
namespace NetFusion.WebApi.Configs
{
    public class GeneralWebApiConfig : IContainerConfig
    {
        public bool UseHttpAttributeRoutes { get; set; } = true;
        public bool UseCamalCaseJson { get; set; } = true;
        public bool UseAutofacFilters { get; set; } = false;
        public bool UseJwtSecurityToken { get; set; } = false;
    }
}
```

When a plugin is being bootstrapped, it can access any of its configuration as follows:

``` csharp
public class WebApiModule : PluginModule
{
    private GeneralWebApiConfig GeneralConfig { get; set; }

    public override void Initialize()
    {
        GeneralConfig = Context.Plugin.GetConfig<GeneralWebApiConfig>();
    }

    // ...
}
```

An host application specifies a configuration by calling the WithConfig method on the ***AppContainer*** before calling its Build method.  The factory method creates a default instance of the configuration class which can have its settings specified.

``` csharp
public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
       var netFusionConfig =  (NetFusionSection)ConfigurationManager.GetSection("netFusion");

        // Configuration Application Container:
        AppContainer.Create(AppDomain.CurrentDomain.RelativeSearchPath)
            .WithConfig(netFusionConfig)

            .WithConfig((GeneralWebApiConfig config) => {

                config.UseHttpAttributeRoutes = true;
                config.UseCamalCaseJson = true;
                config.UseAutofacFilters = true;
                config.UseJwtSecurityToken = true;
            })
            .Load()
            .Start();
    }
}
```

# Implementation
The host application creates a new instance of the ***AppContainer*** by calling the create method and specifying the assembly locations where plug-ins are located.  This is then followed by one or more container configurations.  The build method is called to build the container which locates the Plug-ins, configures them, and builds the Autofac dependency injection container.

Once the host has completed any additional initializations and is ready to start execution, the start method is called.  The start method is the last step and plug-ins are allowed to run any code that may require contacting external resources such as a database (However, this should be kept to a minimum).

## Creation
```C#
AppContainer:Create
```
Factory method that creates an instance of the ***AppContainer*** provided the locations where assemblies containing plug-ins should be searched.  When the container is created, the following internal components are initialized and delegated to:

* ___TypeResolver___
: Responsible for loading types from plug-in assemblies.  This decouples the implementation from interfacing directly with the .NET Assembly.  This allows for easy unit-testing.  The implementation delegates to MEF to find types matching a specified criteria.

* ___CompositeApplication___
: An instance of this class is created to maintain the plug-ins found by the ***AppContainer***.  This class is also responsible for registering types with the dependency injection container.


## Composition
```C#
AppContainer:Build
```
This method executes the bootstrap process that results in the creation of the Autofac dependency injection container.  The process is a follows:

1. Configures Logging that is to be used by ***AppContainer*** based on the host’s configuration.  If a configuration is not specified, a null-logger is used.

2. Next, all plug-in assemblies are identified.  This is completed using MEF by finding and creating all class instances that implement any ***IComposableManifest*** derived interface.  Using the MongoDB plug-in in as an example, a manifest looks as follows:

	``` csharp
	// Search all sources for assemblies containing manifest types
	// identifying application plug-ins and other needed types
	// required to bootstrap the application container.
	private void ImportContainerTypes()
	{
	    var conventions = GetContainerConventions();

	    _typeResover.ImportTypes(conventions, this.Imports);
	    AssertImportedManifests();
	}

	private RegistrationBuilder GetContainerConventions()
	{
	    var builder = new RegistrationBuilder();
	    builder.ForTypesDerivedFrom<IComposableManifest>().Export<IComposableManifest>();
	    builder.ForType<ContainerImports>().ImportProperty(mi => mi.PluginManifests);

	    return builder;
	}
	```

	>The above uses MEF conventions to find all plug-in manifests from all assembly sources.  MEF sets instances of these classes on the PluginManifests property of the ContainerImports POCO instance.  For each found plug-in manifest, a Plug-in POCO is created on the composite application:  

	``` csharp
	private void LoadPlugins()
  {
      _application.Plugins = this.Imports.PluginManifests
          .Select(m => new Plugin(m))
          .ToList();

      _application.Plugins.ForEach(LoadPlugin);
  }
	```

3. After validating the found plug-in manifest types, the ***TypeResolver*** is delegated to and loads all types found in each plug-in.

4. Next, MEF is used to locate and create instances of classes that implement the ***IPluginModule*** interface found in each plug-in.  A given plug-in can have one or more modules.

5. Any host registered ***IContainerConfig*** instances are associated with the plug-in in which they are defined.  The code for the last three steps is as follows:

	``` csharp
	// Find the all types based on conventions within the
	// plug-in required for the bootstrap process.
	private void LoadPlugin(Plugin plugin)
	{
	    var conventions = BuildPlugInBootstrapConventions();
	    _typeResover.LoadPluginTypes(plugin);
	    _typeResover.ImportTypes(plugin.PluginTypes, conventions, plugin);

	    plugin.PluginTypes.ForEach(t => t.Plugin = plugin);
	    plugin.PluginConfigs = plugin.CreatedFrom(_configs.Values).ToArray();

	    MarkKnownTypes(plugin);
	}

	// Conventions for types found within a plug-in used to
	// bootstrap the plugin.
	private RegistrationBuilder BuildPlugInBootstrapConventions()
	{
	    var builder = new RegistrationBuilder();
	    builder.ForTypesDerivedFrom<IPluginModule>().Export<IPluginModule>();

	    builder.ForType<Plugin>().ImportProperty(p => p.PluginModules);
	    return builder;
	}
```

6. After each plug-in has its types loaded, each plug-in type is checked to determine if it is a known type.  A known type implements the ***IKnownType*** marker interface.  This indicates that the plug-in will discover concrete types from other plug-ins that implement the known type. After the known types are identified, the types from other plug-ins, based on known types, are listed as the plugin’s discovered types.

	``` csharp
	private void MarkKnownTypes(Plugin plugin)
	{
	    plugin.PluginTypes.ForEach(pt =>
	    {
	        pt.IsKnownType = pt.Type.IsDerivedFrom<IKnownPluginType>();
	    });
	}

	// For each discovered plug-in known type, find the plug-in
	// defining the known type on which it is based.
	private void SetKnownTypeSources()
	{
	    _application.Plugins.ForEach(SetKnownTypeSources);
	}

	private void SetKnownTypeSources(Plugin plugin)
	{
	    var allPluginTypes = _application.GetPluginTypesFrom();

	    plugin.DiscoveredKnownTypes = plugin.PluginTypes
	        .Where(pt => pt.IsKnownType)
	        .Select(pt => new DiscoveredKnownPluginType
	        {
	            DiscoveredPluginType = pt,
	            DefiningKnownTypePlugins = allPluginTypes
	                .Where(t => pt.Type.IsDerivedFrom(t.Type))
	                .Where(t => !plugin.HasType(t.Type))
	                .Select(t => t.Plugin)
	                .Distinct()
	                .ToList()
	        }).ToArray();
	}
	```

	>Note:  This information is used purely for logging and shows how the application is composed.  Each plug-in can also add additional details to the log.  This log is provided in JSON and viewed as follows:

7. With all the plug-in modules loaded, each module is composed.  This allows each plug-in module to locate concrete types defined in other plug-in assemblies based on abstract types it defines.

	* First, the core plug-ins are composed.  Core plug-ins will discover types in all other types of plug-ins.

	* Then application classified plug-ins are composed.  Application plug-ins will discover types in other application plug-ins only.

8. Each plugin’s module’s SetConventions method is called to determine the module’s abstract defined types.  Then MEF is used to find all implementations.  

	``` csharp
	private void ComposePluginModule(Plugin plugin, IEnumerable<PluginType> sourceTypes)
	{
	    sourceTypes = sourceTypes.ToList();

	    foreach (var module in plugin.PluginModules)
	    {
	        var conventions = new RegistrationBuilder();
	        module.SetConventions(conventions);

	        _typeResover.ImportTypes(sourceTypes, conventions, module);
	    }
	}
	```

	The following is an example from the MongoDB plug-in module that discovers all type mappings and registers them with the MongoDB driver:

	``` csharp
	namespace NetFusion.MongoDB.Modules
	{
	    /// <summary>
	    /// Called when the application is bootstrapped.  Finds all of the
	    /// entity class mapping classes and registers them with MongoDB.
	    /// </summary>
	    internal class MappingModule : PluginModule, IMongoMappingModule
	    {
	        // IMongoMappingModule:
	        public IEnumerable<IEntityClassMap> Mappings { get; private set; }

	        public override void SetConventions(RegistrationBuilder builder)
	        {
	            builder.ForTypesDerivedFrom<IEntityClassMap>().Export<IEntityClassMap>();
	            builder.ForType<MappingModule>().ImportProperty(p => p.Mappings);
	        }

	        // Configures MongoDB driver with mappings.
	        public override void StartModule(IContainer container)
	        {
	            // ...

	            this.Mappings.Select(m => m.ClassMap)
	                .ForEach(BsonClassMap.RegisterClassMap);
	        }

	        // ...

	    }
	}
	```

## Registration
```C#
AppContainer:CreateAutofactContainer
```
With the composite application composed from plug-ins, the next step to have each plug-in module register types with the dependency-injection container.  Registering types in the DI container allows plug-ins to provide services that can be injected into application components at runtime.  The ***AppContainer*** delegates to the ***CompositeApplication*** for type registration.  The steps are as follows:

1. The ***AppContainer*** creates a new Autofac ***ContainerBuilder*** and passes it to *the RegisterComponents* method of the ***CompositeApplication***.

	``` csharp
	private void CreateAutofacContainer()
	{
	    var builder = new Autofac.ContainerBuilder();

	    // Allow all the composite application plug-ins
	    // to register services with container.
	    _application.RegisterComponents(builder);

	    // ...
	}
```

2. The ***RegisterComponents*** method first initializes all plug-in modules.  This allows the plug-in module to initialize anything that might be needed by other plug-in modules configuration methods.  As part of the initialization process, each plug-in module is associated with a ***ModuleContext*** instance that provides information that can be used during the registration process.

3. Next, the second phase of initialization takes place by calling the Configure method on each plug-in module.  This is where each plug-in module can prepare the services that it will register with Autofac.

	``` csharp
	/// <summary>
	/// Populates the dependency injection container with services
	/// implemented by plug-in components.
	/// </summary>
	/// <param name="builder">The DI container builder.</param>
	public void RegisterComponents(Autofac.ContainerBuilder builder)
	{
	    Check.NotNull(builder, nameof(builder));

	    // Note that the order is important.  In Autofac, if a service type
	    // is registered more than once, the last registered component is
	    // used - unless otherwise specified.
	    InitializePluginModules();
	    RegisterCorePluginComponents(builder);
	    RegisterAppPluginComponents(builder);
	}
	```


4. As noted in the above code comments, the order in which plug-ins register their types with Autofac is important.  All core plug-in types should be registered first.  This allows application specific plug-ins to override a given default core registration.  

5. Types are registered with Autofac by calling each plug-in module and invoking the following ***IPluginModule*** methods:

	* ___ScanPluginTypes:___
This method provides each module with a list of types filtered to those contained within its plug-in.  These filtered types are passed to the module as an Autofac ContainerBuilder.

	* ___RegisterComponents:___
This step is invoked by passing the ContainerBuilder directly to each module so they can manually register types.

	* ___ScanOtherPluginTypes:___  This passes to each plug-in module a filtered Autofac ContainerBuilder containing all types from other plug-ins excluding types from its plug-in.

	>Note:  This list of types if filtered based on the type of plug-in.  A core plug-in can scan types from all other core and application plug-ins.  However, a classified application plug-ins can only scan for types contained within another application plug-ins.

6. The ***AppContainer*** is registered as a service within the dependency-injection container.  Any plug-ins implementing a derived ***IPluginModuleSerivce*** interface will automatically be registered as that interface.  Lastly, the host application is called to register any services before the bootstrap process completes.  

	``` csharp
	private void CreateAutofacContainer()
	{
	    // ...

	    // Register additional services,
	    RegisterAppContainerAsService(builder);
	    RegisterPluginModuleServices(builder);
	    RegisterHostProvidedServices(builder);

	    _container = builder.Build();
	    _containerLog = new AppContainerLog(_application, _container.ComponentRegistry);
	}
	```

	>The built DI container can be accessed from AppContainer Services property.  However, this should be rarely used since all object-instances should be resolved from the current lifetime scope.  

## Execution
```C#
AppContainer:Start
```
After the host application completes any additional non-container configuration logic, it needs to call the Start method on the referenced returned from the Build method.  This is the last step and each plug-in will have its StartModule method called.  This is where plug-ins should execute any code that requires calling external resources.  For example, the RabbitMQ plug-in creates any needed exchanges, queues, and consumers.
