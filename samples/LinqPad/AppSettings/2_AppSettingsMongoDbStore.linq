<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Bson.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Bson.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.Core.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.Core.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Eventing.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Eventing.dll</Reference>
  <Reference Relative="..\libs\NetFusion.MongoDB.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.MongoDB.dll</Reference>
  <Reference Relative="..\libs\NetFusion.RabbitMQ.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.RabbitMQ.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.Mongo.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.Mongo.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Newtonsoft.Json.dll</Reference>
  <Reference Relative="..\libs\RabbitMQ.Client.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\RabbitMQ.Client.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>MongoDB.Driver</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Eventing</Namespace>
  <Namespace>NetFusion.Eventing.Config</Namespace>
  <Namespace>NetFusion.MongoDB</Namespace>
  <Namespace>NetFusion.MongoDB.Configs</Namespace>
  <Namespace>NetFusion.MongoDB.Testing</Namespace>
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Mongo</Namespace>
  <Namespace>NetFusion.Settings.Mongo.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
  <Namespace>Samples.Domain.RabbitMQ.Events</Namespace>
</Query>

// *************************************************************************
// This query shows examples of using a custom settings initializer to load
// application settings from MongDB. 
// *************************************************************************

void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new HostTypeResolver(pluginDirectory,
		"NetFusion.Settings.dll", 
		"NetFusion.MongoDB.dll", 
		"NetFusion.Settings.Mongo.dll")
	{
		LoadAppHostFromAssembly = true
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		config.AddPlugin<LinqPadHostPlugin>();
	})
	.WithConfig<NetFusionConfig>(config =>
	{
		// Configure the MongoDB settings initializer.
		config.AddSettingsInitializer(typeof(MongoSettingsInitializer<>));
		
	}).WithConfig<MongoAppSettingsConfigSection>(config => {

		config.MongoAppSettingsConfig = new MongoStoreConfigElement
		{
			// Override the default MongoDb settings values.  If not specified,
			// the default values specified in code are used.  In a production
			// application, MongoAppSettingsConfigSection would be populated 
			// from the application configuration file by using WithConfigSection.
			MongoUrl = "mongodb://localhost:27017",
			DatabaseName = "LinkPadTestDb",
			CollectionName = "Example.Settings"
		};
	})
	.Build()
	.Start();

	// Execute the examples:
	InsertExampleSettingsInMogoForExample();
	RunMongoDbSettingsStorageExample();
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{
	public LinqPadHostPlugin()
	{
		this.PluginId = "570b8fbcb35499490dfad4e5";
	}
}

// This settings class is not needed for the settings plug-in.
// It is just being used to add as settings document to the
// MongoDb collection that stores app settings.
public class LinqPadTestDb : MongoSettings
{
	public LinqPadTestDb()
	{
		this.IsInitializationRequired = false;
		this.DatabaseName = "LinkPadTestDb";
		this.MongoUrl = "mongodb://localhost:27017";
	}
}

public void InsertExampleSettingsInMogoForExample()
{
	var db = AppContainer.Instance.Services.Resolve<IMongoDbClient<LinqPadTestDb>>();
	var settingsColl = db.GetCollection<AppSettings>();
	
	// Drop the collection containing the settings:
	db.DropCollectionAsync<AppSettings>().Wait();
	
	// Insert new settings into collection.
	settingsColl.InsertOneAsync(new TestSettings()).Wait();
}

public void RunMongoDbSettingsStorageExample()
{
	nameof(RunMongoDbSettingsStorageExample).Dump();

	var testSettings = AppContainer.Instance.Services.Resolve<TestSettings>();
	testSettings.Dump();
}

public class TestSettings : AppSettings
{
	public TestSettings()
	{
		this.ApplicationId = "570b8fbcb35499490dfad4e5";
		this.Environment = EnvironmentTypes.Development;
	}
	
	public int Value1 { get; set; } = 100;
	public int Value2 { get; set; } = 200;
}