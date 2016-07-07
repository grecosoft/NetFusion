# Bootstrap Overview
The bootstrap process configures the application-container from plug-ins identified by a plug-in manifest.  A plug-is is nothing more than an assembly containing classes based on a given set of conventions.  These convention based classes are instantiated during the bootstrap process and used to initialize the application container.  The end result of the bootstrap process is a constructed Autofac dependency-injection container that can be used by the host application.

## Container Log
The following is an example of the log showing how the application is composed.  Allows a developer to have a visual look at the application.

![image](../../../img/NetFusionLog-1.png)
![image](../../../imgNetFusionLog-2.png)
    
The design is based on the following:

## Plugin Manifest Types
Identifies the application assemblies that should be included in the bootstrap process.  There are three types of composite manifests:

* ___Application Host___
: The assembly that is the execution host of the application.  This can be any of the common .NET application types (WebApi, MVC, Windows Service, Console Application, ...).

* ___Application Component___
: Application specific assemblies.  This can be assemblies containing an application’s infrastructure code such as application servers and repositories.  Also included are an application’s business domain specific assemblies.  

* ___Core Plugin___
: Assemblies containing reusable technology centric plug-ins used by other core or application component plug-ins.

### Known-Type
A types that is defined by a given plug-in but whose implementations are discovered in assemblies participating in the bootstrap process.

### Concrete Know-Type
A concrete implementation of a defined known-type used to integrate with the plug-in defining the abstract known-type. 

### Discovering Plugin
The plug-in defining the abstract known-type discovers concrete implementations and is provides instances automatically as part of the bootstrap process.

### Plug-in Module
A class containing initialization code within a plug-in called at specific steps during the bootstrap process.  A module is responsible for configuring its application container specific functionality and registering needed services within the dependency-injection container.


# Setup
The minimal bootstrap configuration for an application hose follows:

1. Add the following core Nuget packages to the host project:

	![image](../../../img/Nuget-NetFusion.Bootstrap.png)
	![image](../../../img/Nuget-NetFusion.Settings.png)

2. Add the following to the host’s configuration file:

``` xml

	<?xml version="1.0" encoding="utf-8"?>
	<configuration>
		<configSections>
			<section name="netFusion" type="NetFusion.Settings.Configs.NetFusionConfigSection, NetFusion.Settings" />
		</configSections>
		<netFusion>
			<hostConfig environment="Development" />
		</netFusion>
	</configuration>
```

3. Define the application host manifest:

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
	
	using NetFusion.Bootstrap.Manifests;

	namespace Samples.WebHost
	{
    	public class Manifest : PluginManifestBase,
        	IAppHostPluginManifest
    	{
        	public string PluginId => "56b68f27a694e92f6ca03ee6";
        	public string Name => "Sample Web Host";
        	public string Description => "Example host providing examples.";
    	}
}
```	

# Logging
The host application can specify a logger to be used.  During the bootstrap process, the AppContainer will log any exceptions before they are raised to the caller.  If a logger is not specified, then the calling application host is responsible for logging exceptions that are raised.  The application host can configure its logger of choice.

The following shows how the container can be configured to use Serilog.

1. Add the following integration NuGet package to the host project:

	![image](../../../img/Nuget-NetFusion.Logging.Serilog.png)

2. Logging is added to the ***AppContainer*** as follows:

	* Create the Serilog logger.
	* Add the ***LoggerConfig*** container configuration and specify the logger.
	* Add the ***AutofacRegistrationConfg*** container configuration and register the logger.
	
	(TOTO-check code)
	
	After logging has been configured, the bootstrap code should looks as follows:
	
	``` csharp
	
		// Create logger:
        var logConfig = new LoggerConfiguration()
        	.WriteTo.Seq("http://localhost:5341");
        	
        logConfig.MinimumLevel.Debug();
        var logger = logConfig.CreateLogger();

        // Configuration Application Container:
        AppContainer.Create(new[] { "Samples.*.dll" })
    		.WithConfigSection("netFusion")

            .WithConfig((LoggerConfig config) => {
            
                    config.LogExceptions = true;
                    config.SetLogger(new PluginLogger(logger));
            })

            .WithConfig((AutofacRegistrationConfig config) => {
                   
                   config.Build = builder => {
               		
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

A plugin may need configurations specified by the host application for certain settings.  Plugins define configurations by deriving a class from the ***IContainerConfig*** marker interface.  

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

When a plugin is being bootstrapped, it can access any of its configurations as follows:

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

If the NetFusion configuration section is defined within he host's application configuration file, it can be added to the container by calling the **WithConfigSection** method.  This method accepts one or more configuration section names that are to be added to the container.  The following is an example:

``` csharp

	AppContainer.Create(new[] { "Samples.*.dll" })
    	.WithConfigSection("netFusion", "mongoAppSettings")
         
        // Eventing Plug-in Configuration.
        .WithConfig((MessagingConfig config) =>
        {
        	config.AddMessagePublisherType<RabbitMqMessagePublisher>();
        })

        // Configure Settings Plug-in.  This tells the plug-in where to look for
        // injected application settings.
        .WithConfig((NetFusionConfig config) => {

        	config.AddSettingsInitializer(
            	typeof(FileSettingsInitializer<>),
                typeof(MongoSettingsInitializer<>));
         	})
           .Build()
           .Start();

```

An host application specifies a configuration by calling the WithConfig factory method on the ***AppContainer*** before calling its Build method.  The factory method creates a default instance of the configuration class which can have its settings optionally specified.

``` csharp

	AppContainer.Create(new[] { "Samples.*.dll" })
    	.WithConfigSection("netFusion")
		.WithConfig((GeneralWebApiConfig config) => {

        	config.UseHttpAttributeRoutes = true;
            config.UseCamalCaseJson = true;
            config.UseAutofacFilters = true;
            config.UseJwtSecurityToken = true;
    	})
        .Load()
        .Start();
            
```

# Implementation Details
The host application creates a new instance of the ***AppContainer*** by calling the **Create** method and specifies the pattern used for searching assemblies representing plug-ins.  The first of the following two directories with a value is searched by the default type resolver:  

* AppDomain.CurrentDomain.RelativeSearchPath
* AppDomain.CurrentDomain.BaseDirectory

This is then followed by one or more container configurations.  The ***Build*** method is called to build the container which locates the Plug-ins, configures them, and builds the Autofac dependency injection container.

Once the host has completed any additional initializations and is ready to start execution, the container's ***Start*** method is called.  The start method is the last step and plug-ins are allowed to run any code that may require contacting external resources such as a database or creating needed exchanges and queues. (However, this should be kept to a minimum).

## Creation

When the **Create** method is invoked on the container, the following internal components are initialized and delegated to:

* ___TypeResolver___
: An instance of this class is responsible for the following:

	* Locating the assemblies representing plug-ins
	* Loading each plug-in's associated types
	* Discovering concrete instances of each plug-in's defined known-types.
   
* ___CompositeApplication___
: An instance of this class is created to maintain the plug-ins found by the ***AppContainer***.  This class is also responsible for registering types with the dependency injection container.  This is just an in-memory representation of the container's structure.


## Composition

This method executes the bootstrap process that results in the creation of the Autofac dependency injection container.  The following process takes place when the ___Build___ method of the ***AppContainer*** is invoked:

1. Configures logging that is to be used by ***AppContainer*** based on the host’s configuration.  If a configuration is not specified, a null-logger is used.  If the container is being hosted in LinqPad for incremental development where a real longer might not be specified, the AppConainer will write all messages to a string collection contained within the null-logger.  The logger instance can be referenced using the ***Logger*** property of the ***AppContainer*** instance.

2. A plug-in assembly is identified by containing a manifest class.  A manifesto class is just a POCO implementing one of the following derived ***IPluginManifest*** interfaces:

	* IAppHostPluginManifest: The assembly representing the application host.
	* IAppComponentManifest: One or more assemblies containing application specific business logic.
	* ICorePluginManifest: One ore more assemblies containing reusable infrastructure implementations. 

The concrete manifest class also contains information describing the plug-in.  
	  
All plug-in assemblies are identified by delegating to the ***TypeResolver*** and calling the ***DiscoverMenifests*** method. 
This method populates the ***AllMenifests*** property of the ***ManifestRegistry*** object with instances of all discovered manifests.  The found manifests are then validated.  


``` csharp
	
	// Search all assemblies representing plug-ins.
    private void LoadManifestRegistry()
    {
	    _typeResover.DiscoverManifests(this.Registry);

        AssertManifestProperties();
        AssertUniqueManifestIds();
        AssertLoadedManifests();
    }
    
```

For each found plug-in manifest, a Plug-in POCO is created on the composite application and associated with the manifest:

	``` csharp
	
		// For each found plug-in manifest assembly, create a plug-in instance
        // associated with the manifest and add to composite application.
        private void LoadPlugins()
        {
            _application.Plugins = this.Registry.AllManifests
                .Select(m => new Plugin(m))
                .ToArray();

            _application.Plugins.ForEach(LoadPlugin);
        }
	```

3. After validating the found plug-in manifest types, the ***TypeResolver*** is delegated to and loads all types found in each plug-in.  Not having the type-resolver and not the application-container load a plug-ins types from its corresponding assembly decouples the application-container from the .NET runtime.  This makes unit-testing and hosting in other environments such as LinqPad much easier.

4. Next, a request is made to the type-resolver to have the modules of each plug-in instantiated.  The type-resolver populates the ***PluginModules*** property for each plug-in with ***IPluginModule*** instances.  A given plug-in can have one or more modules.

5. Any host registered ***IContainerConfig*** instances are associated with the plug-in in which they are defined.  The code for the last three steps is as follows:

	``` csharp
	
		private void LoadPlugin(Plugin plugin)
        {
            _typeResover.LoadPluginTypes(plugin);
            _typeResover.DiscoverModules(plugin);

            plugin.PluginConfigs = plugin.CreatedFrom(_configs.Values).ToList();
        }
	```

6. With all the plug-in modules loaded, each module is composed by having all public ***IEnumerable*** properties based on a ***IPluginKnownType*** type populated with defined concrete instances.  This allows each plug-in module to locate concrete types defined in other plug-in assemblies (including its assembly) based on abstract types it defines.  The type of plug-in determines which other plug-ins are searched for concrete ***IPluginKnowType*** types.

	* Core plug-ins are composed from types declared in all plug-ins.  Core plug-ins provide infrastructure services that can be used by all plug-in types (including other core plug-ins.)

	* Then application classified plug-ins are composed.  Application plug-ins will discover types in other application plug-ins only.  Application centric plug-ins are higher-level and never provide services to lower-level plug-in types.

The following code accomplishes the above:

``` csharp

	// Core plug-in modules discover search for known-types contained within all plug-ins.
    private void ComposeCorePlugins()
    {
    	var allPluginTypes = _application.GetPluginTypesFrom();

        _application.CorePlugins.ForEach(p =>
            ComposePluginModules(p, allPluginTypes));
    }

    // Application plug-in modules search for known-types contained only within other 
    // application plug-ins.  Core plug in types are not included since application plug-ins
    // never provide functionality to lower level plug-ins.
    private void ComposeAppPlugins()
    {
        var allAppPluginTypes = _application.GetPluginTypesFrom(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

        _application.AppComponentPlugins.ForEach(p =>
            ComposePluginModules(p, allAppPluginTypes));

        ComposePluginModules(_application.AppHostPlugin, allAppPluginTypes);
    }
```

The following shows the MongoDB mapping module containing an enumerated property of ***IEntityClassMap*** types.  When the module is loaded, this property will automatically be populated with instances of all ***IEntityClassMap*** concreate types.   

``` csharp

	internal class MappingModule : PluginModule, IMongoMappingModule
    {
        // IMongoMappingModule:
        public IEnumerable<IEntityClassMap> Mappings { get; private set; }
    	
    	// ...    
    }
```

The ***AppContainer*** contains the following code that composes each plug-in module's known type enumerable properties:

``` csharp

	private void ComposePluginModules(Plugin plugin, IEnumerable<PluginType> fromPluginTypes)
    {
    	var pluginDiscoveredTypes = new HashSet<Type>();
        foreach (var module in plugin.PluginModules)
        {
            var discoveredTypes = _typeResover.DiscoverKnownTypes(module, fromPluginTypes);
            discoveredTypes.ForEach(dt => pluginDiscoveredTypes.Add(dt));
        }

        plugin.SearchedForKnowTypes = pluginDiscoveredTypes.ToArray();
     }
```

## Initialize/Configure/Registration
With the composite application composed from plug-ins, the next step to have each plug-in module register types with the dependency-injection container.  Registering types in the DI container allows plug-ins to provide services that can be injected into application components at runtime.  The ***AppContainer*** delegates to the ***CompositeApplication*** for type registration.  The steps are as follows:

1. The ***AppContainer*** creates a new Autofac ***ContainerBuilder*** and passes it to the ***RegisterComponents*** method of the ***CompositeApplication***.

``` csharp

	private void CreateAutofacContainer()
    {
		var builder = new Autofac.ContainerBuilder();

		// Allow the composite application plug-ins
		// to register services with container.
		_application.RegisterComponents(builder);

		// Register additional services,
		RegisterAppContainerAsService(builder);
		RegisterPluginModuleServices(builder);
		RegisterHostProvidedServices(builder);

		_container = builder.Build();
	}
	
```

2. The ***RegisterComponents*** method first initializes all plug-in modules.  This allows the plug-in module to initialize anything that might be needed by other plug-ins and can be accessed within their configuration methods  As part of the 3 phase initialization process, each plug-in module is associated with a ***ModuleContext*** instance that provides information that can be used during the initialization, configuration, and registration process.

3. Next, the second phase of initialization takes place by calling the ***Configure*** method on each plug-in module.  This is where each plug-in module can configure itselft and/or prepare the services that it will register with Autofac.

4. Once configuration is completed, each plug-in module is allowed to register components that should be services within the dependency-injection container.  The code is as follows: 

``` csharp

	/// <summary>
    /// Populates the dependency injection container with services
    /// registered by plug-in modules.
    /// </summary>
    /// <param name="builder">The DI container builder.</param>
    public void RegisterComponents(Autofac.ContainerBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // Note that the order is important.  In Autofac, if a service type 
        // is registered more than once, the last registered component is
        // used.  This is the default configuration.
        InitializePluginModules();
        RegisterCorePluginComponents(builder);
        RegisterAppPluginComponents(builder);
    }

```

5. As noted in the above code comments, the order in which plug-ins register their types with Autofac is important.  All core plug-in types should be registered first.  This allows application specific plug-ins to override a given default core plug-in registration.  

6. Types are registered with Autofac by calling each plug-in module and invoking the following ***IPluginModule*** methods:

	* ___ScanPluginTypes:___
This method provides each module with a list of types filtered to those contained within its plug-in.  These filtered types are passed to the module as an Autofac ContainerBuilder.

	* ___RegisterComponents:___
This step is invoked by passing the ContainerBuilder directly to each module so types can be manually registered.

	* ___ScanOtherPluginTypes:___  
This passes to each plug-in module a filtered Autofac ContainerBuilder containing all types from other plug-ins excluding types from its plug-in.  Note:  This list of types if filtered based on the type of plug-in.  A core plug-in can scan types from all other core and application plug-ins.  However, an application plug-ins can scan only types contained within another application plug-ins.

	* ___ScanApplicationPluginTypes:___
This method is called to allow core-plugins to limit its scanning only to types found in application plug-ins. 

The following is the code that executes each of the above methods:		
``` csharp

	private void RegisterCorePluginComponents(Autofac.ContainerBuilder builder)
    {
    	var allPluginTypes = GetPluginTypesFrom();
        foreach (var plugin in this.CorePlugins)
        {
        	ScanPluginTypes(plugin, builder);
            RegisterComponents(plugin, builder);
            ScanOtherPluginTypes(plugin, builder, allPluginTypes);
            ScanApplicationPluginTypes(plugin, builder);
        }
	}
```
	

7. Next, additional servers are registered then the dependency-injection container is built.  The ***AppContainer*** is registered as a singleton service within the dependency-injection container.  Any plug-ins implementing a derived ***IPluginModuleSerivce*** interface will automatically be registered as a singleton services for that interface.  Lastly, the host application is called to register any services before the bootstrap process completes.  

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

Lastly, the built DI container is built and can be accessed from ***AppContainer*** ___Services___ property.  However, this should be rarely used since all object-instances should be resolved from the current lifetime scope by constructor-injection.

## Execution
After the host application completes any additional non-container configuration logic, it needs to call the Start method on the reference returned from the ___Build___ method.  This is the last step and each plug-in will have its ___StartModule___ method called.  This is where plug-ins should execute any code that requires calling external resources.  For example, the RabbitMQ plug-in creates any needed exchanges, queues, and consumers.
