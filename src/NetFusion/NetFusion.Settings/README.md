# Settings Overview
>Settings are classes containing properties, loaded at runtime, that can be injected into  components.  Setting initializer strategies determine how a given settings object is loaded.  

For example, the MongoDB plugin defines the ***MongoSettings*** class from which application specific database configurations derive.

``` csharp
/// <summary>
/// Setting uses by the MongoDB client when connecting
/// to the database.
/// </summary>
public abstract class MongoSettings : AppSettings
{
    /// <summary>
    /// The URL used by the client when connecting to
    /// the database.
    /// </summary>
    /// <returns></returns>
    public string MongoUrl { get; set; }

    /// <summary>
    /// User name used to authenticate with the database.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Password used to authenticate with the database.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The database used to authenticate.
    /// </summary>
    public string AuthDatabaseName { get; set; }

    // ...
}
```

The following is an example of a derived settings class used to specify properties of the database.

``` csharp
public class InvoiceDb : MongoSettings
{
	public InvoiceDb()
	{
		this.MongoUrl = "mongodb://localhost:27017";
		this.DatabaseName = "Invoicing";
	}
}
```

Using the ***InvoiceDb*** settings class above, the settings are loaded when the client injects the following for the first time into a component:

``` csharp
IMongoDbClient<InvoiceDb> invoiceDb....
```
The following sections will show how to store these settings external to the application.

# Setup
Application specific settings can be defined as in this example:

1. Define an ***AppSettings*** derived class that specifies the settings properties.

2. Access the settings by injecting them into a component.  Since a console application is being used for this documentation, the container will be accessed directly since it is not part of the application’s pipeline as it is for a WebApi project.

If you look at the return settings variable in the debugger, you will see that it has the values provided in the code above.  The default specified code values were used since no ***IAppSettingsInitializer*** strategy instances were specified as part of the bootstrap process.  This is usually not what is intended so the setting plugin writes a warning to the log file:

Next, the bootstrap configuration will have setting initializer strategies added.  The following two custom strategies are available:

* ___FileSettingsInitializer___
: loads from a local file path.  Can be good for development to override global team settings.

* ___MongoSettingsInitializer___
: loads from a MongoDb collection.

Add the following code to the application’s bootstrap:

``` csharp
protected void Application_Start()
{
    var netFusionConfig = (NetFusionSection)ConfigurationManager.GetSection("netFusion");

    AppContainer.Create(AppDomain.CurrentDomain.RelativeSearchPath)
        .WithConfig(netFusionConfig)

        // Configure Settings Plugin.  This tells the plugin where to look for
        // injected application settings.
        .WithConfig((AppConfig config) => {

            config.AddSettingsInitializer(
                typeof(FileSettingsInitializer<>),
                typeof(MongoSettingsInitializer<>));
        })

        .Build()
        .Start();
}
```

The first strategy that can satisfy the request will be used, therefore, the configuration order is important.  

## File Settings Initializer
The ***FileSettingsInitializer*** locates a JSON file with the same name as the settings class within the host’s Configs directory.  The file must be located in a subdirectory corresponding to the configured application’s environment.  The following is an example:

Also, a developer can specify their settings within a directory consisting of their computer's name.  This directory then has child folders for each environment containing the configuration files.  This is useful if a developer wants to override a teams global settings during development.

## MongoDB Settings Initializer
The second strategy allows for settings to be stored within MongoDB.  If a configuration is not specified, the default values specified in code will be used:

``` csharp
public class MongoSettingsConfig : IContainerConfig
{
    internal string MongoUrl { get; set; } = "mongodb://localhost:27017";
    internal string DatabaseName { get; set; } = "NetFusion";
    internal string CollectionName { get; set; } = "NetFusion.AppSettings";
}
```

To override these settings, place the following within the host’s application configuration file:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="netFusion" type="NetFusion.Settings.Configs.NetFusionSection, NetFusion.Settings" />
  </configSections>

  <netFusion>
    <hostConfig environment="Development" />

    <mongoAppSettingsConfig mongoUrl="mongodb://localhost:27017" databaseName="NetFusion" collectionName="NetFusion.AppSettings" />
  </netFusion>
</configuration>
```

The corresponding document within MongoDB for ExampleSettings would be configured as follows for the development environment:


# Implementation
For the majority of application settings, the values should be stored external to the application so they can be easily maintained.  It is important to note that application settings are not initialized as part of the bootstrap process.  Only the information needed to populate them is determined during the bootstrap process.  The NetFusion.Settings plugin scans for all ***IAppSettings*** derived classes and registers them in the dependency injection container as singletons.  Application settings are loaded on demand when requested for the first time and remain in memory.  

The NetFusion.Settings plugin also does not contain any code  determining form where settings are loaded.  It provides the ***IAppSettingsInitializer*** interface that can be implemented to specify a load strategy.  If no ***IAppSettingsInitializer*** implementations are found, the default instance of the settings class is returned having the default values as specified in code.

When the settings are injected into a component for the first time, the following determines which ***IAppSettingsInitializer*** load strategy is used:

1. If a specific ***IAppSettingsInitializer*** closed generic type exists for the settings class, it has the highest priority and is used.  During the bootstrap process an exception is raised if more than one setting initializer exists for a specific settings type.

2. If no specific settings initializer is found, all open-generic setting initializers configured during the bootstrap process are checked.  Each generic settings initializer is checked in the order they are configured and stops at the first strategy that was able to load the settings.

3. If no strategy was found by the first two steps, a default instance of the setting class is returned and a warning is written to the log.

Settings are also associated with an development environment.  This value can be one of the following:
* Dev
* Test
* Prod
