# Settings Overview
Settings are classes containing properties, loaded at runtime, that can be injected into components.  Setting initializer strategies determine how a given settings object is loaded.  

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

The following is an example of a derived settings class used to specify properties of a specific database.

``` csharp

	public class InvoiceDb : MongoSettings
	{
		public InvoiceDb()
		{
			this.IsInitializationRequired = false;
			this.MongoUrl = "mongodb://localhost:27017";
			this.DatabaseName = "Invoicing";
		}
	}
```

Using the ***InvoiceDb*** settings class above, the settings are loaded when the client injects the following for the first time into a component:


``` csharp

	SomeConstructor (IMongoDbClient<InvoiceDb> invoiceDb) { ... }
```
The following sections will also show how to store these settings external to the application.

# Setup
The base application settings implementation is within the NetFusion.Settings assembly and its corresponding Nuget package is installed as follows:

![image](../../../img/Nuget-NetFusion.Settings.png)

Application specific settings are defined as follows:

1. Define an ***AppSettings*** derived class that specifies the settings properties (InvoiceDb).

2. Access the settings by injecting them into a component.

``` csharp

	SomeConstructor (IMongoDbClient<InvoiceDb> invoiceDb) { ... }
```

If you look at the return settings variable in the debugger, you will see that it has the values provided in the code above.  The default specified code values were used since the ___IsInitializationRequired___ is specified as False and no ***IAppSettingsInitializer*** strategy instances were declared or specified as part of the bootstrap process.  

This is usually not what is intended and setting the ___IsInitializationRequired___ property to True (the default value) will result in an exception being raised if no ***IAppSettingsInitializer*** can be found to iniialize the settings.

There can be two types of setting initialization strategies:

* ___Setting Sepecific___
: These are initializers that implement the ***IAppSettingsInitializer*** for a specific setting type.  There can be only one setting initializer for a specific setting type.  If more than one is found, an exception will be thrown during the bootstrap process.  Only the application plug-in types are searched for this type of initializer.  The following would be an example:  

``` csharp

	public class TestInitializer : AppSettingsInitializer<TestSettings2>
	{
		protected override IAppSettings OnConfigure(TestSettings2 settings)
		{
			settings.Value1 *= 2;
			settings.Value2 *= 2;
		
			// Initialize the values as above or return a new initialized
			// instance of the class.  If the initializer cannot satify the
			// request, it should return null.
		
			return settings;
		}
	}```

* ___Generic Initializer___
: This type of settings initializer is implemented as an open-generic type and can be used to load any type of application setting.  Such examples are as follows:


	* ___FileSettingsInitializer___
	: loads from a local file path.  Can be good for development to override global team settings.

	* ___MongoSettingsInitializer___
	: loads from a central MongoDB collection.

Generic initializers are configured as follows:

``` csharp
	
        // Configuration Application Container:
        AppContainer.Create(new[] { "Samples.*.dll" })
    		.WithConfigSection("netFusion")
    		
    		.WithConfig<NetFusionConfig>(config =>
			{
				// Configure the MongoDB settings initializer.
				config.AddSettingsInitializer(
					typeof(FileSettingsInitializer<>
					typeof(MongoSettingsInitializer<>));
			})
            .Load()
            .Start();
        }
    }
```

The setting initializers are processed as follows:

1.  If there is a settings specific initializer, it will first be executed.
	
2.  if there is no settings specific initializer or if it could not satisfy the request, the registered open-generic settings initializers are tested.
	
3.  Each generic settings initializer is tried and execution stops after finding the first instance that can satisfy the request.
	
4.  If no setting initializers are found that could satisfy the request, the setting's ___IsInitializationRequired___ property is checked.  This value defaults to True so uninitialized settings are not used in error.  If this property is overridden to be False, the object with its property values set in code will be returned.  If the property has the default value of True, an exception is thrown.


For the majority of application settings, the values should be stored external to the application so they can be easily maintained.  It is important to note that application settings are not initialized as part of the bootstrap process.  Only the information needed to populate them is determined during the bootstrap process.  

The NetFusion.Settings plugin scans for all ***IAppSettings*** derived classes and registers them in the dependency injection container as singletons.  Application settings are loaded on demand when requested for the first time and remain in memory.  


## File Settings Initializer
The ***FileSettingsInitializer*** locates a JSON file with the same name as the settings class within the host’s Configs directory.  The file must be located in a subdirectory corresponding to the configured application’s environment.  The following is an example:

Also, a developer can specify their settings within a directory consisting of their computer's name.  This directory then has child directories for each environment containing the configuration files.  This is useful if a developer wants to override a teams global settings during development.
