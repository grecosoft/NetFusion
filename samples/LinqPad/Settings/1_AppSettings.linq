<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Newtonsoft.Json.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
</Query>

/// <summary>
/// A plug-in defines application settings by deriving a POCO from the base AppSetings class.  
/// This class contains properties with settings specific to the host application or plug-in
/// defining the settings class.  Multiple application-settings classes can be defined by a 
/// plug-in.  IAppSettingsInitializer implementations determine how the application settings
/// classes are populated.  The settings classes are initialized when injected into a dependent
/// component for the first time.  The setting classes are registered within the container as 
/// singletons. 
/// </summary>
void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new TestTypeResolver(pluginDirectory, 
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
// property has a value of True, an exception will be thrown when the settings instance is
// injected into a dependent component and there is not a associated settings initializer.
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