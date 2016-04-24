# MongoDB Setting Overview
The NetFusion.Settings.MongoDB assembly provides an implementation of an application settings initializer that will load settings stored in MongoDB.  The ___MongoSettingsInitializer___ can be used along with the ___FileSettingsInitializer___ so developers can override global settings stored in MongoDB.  For this scenario, the ___FileSettingsInitializer___ must be registered before the ___MongoSettingsInitializer___ during setup.

##Setup
Install the following NetFusion plugin:

![image](../../../img/Nuget-NetFusion.Settings.MongoDB.png)

The application container is bootstrapped is as follows:

``` csharp
	
        // Configuration Application Container:
        AppContainer.Create(new[] { "Samples.*.dll" })
    		.WithConfigSection("netFusion", "mongoAppSettings")
    		
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

The configuration also needs to specify the MongoDB server and collection where the settings are located.  If this is not specified, the following default settings are used:

``` sharp

	/// <summary>
    /// Class containing the settings used to load applications settings
    /// from MongoDB.
    /// </summary>
    public class MongoAppSettingsConfig : IContainerConfig
    {
    	public MongoAppSettingsConfig() 
    	{
	    	this.IsInitializationRequired = false;
    	}
    
        public string MongoUrl { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "NetFusion";
        public string CollectionName { get; set; } = "NetFusion.AppSettings";
    }
```

The default settings are used to configure the plug-in running locally for testing.  For a more established application, these setting will be read from a central MongoDB instance.  The above settings can be specified within the application's configuration file as follows:

``` xml

	<?xml version="1.0" encoding="utf-8"?>
	<configuration>
		<configSections>
    		<section name="netFusion" type="NetFusion.Settings.Configs.NetFusionConfigSection, NetFusion.Settings" />
    		<section name="mongoAppSettings" type="NetFusion.Settings.Mongo.Configs.MongoAppSettingsConfigSection, NetFusion.Settings.Mongo" />
    	</configSections>
    	
    	<netFusion>
    		<hostConfig environment="Development" />
    	</netFusion>
    	
    	<mongoAppSettings>
    		<settingsStore mongoUrl="mongodb://localhost:27017" databaseName="NetFusion" collectionName="NetFusion.AppSettings" />
    	</mongoAppSettings>
    </configuration>
```

Any or all of the settings can be specified.  For any settings not specified, the default values specified above are used.  Note that the configuration section is added to the container using the ___WithConfigSection___ container configuration method.

Each setting class is stored as a document in MongoDB.  To define a new settings class to be stored in MongoDB, only the class needs to be declared.  This plug-in's module automatically handles configuring the needed MongoDB client mappings and known types.  The following is an example of a settings class document in MongoDB.  Note that settings can be associated with an optional environment and computer.  If no environment is specified, the settings apply to all environments.