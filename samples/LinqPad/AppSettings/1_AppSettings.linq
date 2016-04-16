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
  <Reference Relative="..\libs\Samples.Domain.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Samples.Domain.dll</Reference>
  <Namespace>Autofac</Namespace>
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
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
  <Namespace>Samples.Domain.RabbitMQ.Events</Namespace>
  <Namespace>MongoDB.Driver</Namespace>
</Query>

// *************************************************************************
// This query shows examples of using the Settings Plug-in for loading 
// application specific settings.  These examples assume that the settings 
// are being configured directly in memory by the application host.  The 
// example queries in 4_NetFusionMongoSettingsExamples.linq shows how to 
// load the settings from a MongoDb collection.
// *************************************************************************
void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new HostTypeResolver(pluginDirectory, 
		"NetFusion.Settings.dll")
	{
		LoadAppHostFromAssembly = true
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		config.AddPlugin<LinqPadHostPlugin>();
	})
	.WithConfig<NetFusionConfig>(config => {
		config.AddSettingsInitializer(typeof(TestOpenGenericInitializer<>));
	})
	.Build()
	.Start();
	
	// Execute the examples:
	RunNonInitializedSettingsExample();
	RunInitializedSettingsExample();
	RunGenericInitializerExample();
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

// *****************************************************************************************
// Non-Initialized Settings:  A plug-in defines application settings by deriving a POCO
// from the base AppSetings class.  This class contains properties with settings specific
// to the plug-in defining the settings class.  Multiple application-settings classes can
// be defined by a plug-in.  This example assumes that the default property values for the 
// defined settings class are to be used without initialization by the host application.
//
// If the host application doesn't have to define a settings initializer class, the 
// settings class must set the IsInitializationRequired property to False.  This property 
// defaults to True to prevent the accidental use of non-initilized settings.  If this 
// property has a value of True, an exception will be thrown when the settings instance 
// is injected into a dependent component and there is not associated settings initializer.
// *****************************************************************************************
public void RunNonInitializedSettingsExample()
{
	nameof(RunNonInitializedSettingsExample).Dump();

	var testSettings = AppContainer.Instance.Services.Resolve<TestSettings>();
	testSettings.Dump();
}

public class TestSettings : AppSettings
{
	public TestSettings()
	{
		this.IsInitializationRequired = false;
	}
	
	public int Value1 { get; set; } = 100;
	public int Value2 { get; set; } = 200;
}

// *****************************************************************************************
// The host or application centric plug-in can define an initializer for a defined 
// application settigs class.  Initializers can be for a specific application settings
// class (closed-generic) or applicable to any application setting (open-generic).  If a
// settings specfic initializer is found and is able to intialize the setting, it is used.
// If there is no defined specfic initializer or one is defined and it can't saftify the
// request, all open-generic intializers are processed.  The first found open-generic 
// initializer (in configuration order) that can satify the request will be used.
// *****************************************************************************************
public void RunInitializedSettingsExample()
{
	nameof(RunInitializedSettingsExample).Dump();

	var testSettings = AppContainer.Instance.Services.Resolve<TestSettings2>();
	testSettings.Dump();
}

public class TestSettings2 : AppSettings
{
	public int Value1 { get; set; } = 500;
	public int Value2 { get; set; } = 700;
}

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
}

// *****************************************************************************************
// The following is an example of a configured open-generic initializer.  An open-generic
// initializer can be used to configure multiple application settings instances.  This is
// how the MongoDb settings initializer is implemented.
// *****************************************************************************************
public void RunGenericInitializerExample()
{
	nameof(RunGenericInitializerExample).Dump();

	var testSettings = AppContainer.Instance.Services.Resolve<TestSettings3>();
	testSettings.Dump();
}

public class TestSettings3 : AppSettings
{
	public int Value1 { get; set; } = 1000;
	public int Value2 { get; set; } = 2000;
}

public class TestOpenGenericInitializer<TSettings> : AppSettingsInitializer<TSettings>
	where TSettings : IAppSettings
{
	protected override IAppSettings OnConfigure(TSettings settings)
	{
		var initSettings = settings as TestSettings3;
		if (initSettings != null)
		{
			initSettings.Value1 += 100;
			initSettings.Value2 += 200;
			return settings;
		}
		return null;
	}
}