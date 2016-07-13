<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Settings.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
</Query>

// ******************************************************************************************
// The following creates a container within LinqPad that will automatically scan a specified 
// directory for assemblies containing plug-ins.  This is also how a container is used within
// an actual host outside of LinqPad  such as WebApi.  In addition, all types defined within
// the LinqPad query window will be automatically assocated with the defined application host
// plug-in (LinqPadHostPlugin in this example).
// ******************************************************************************************

// -------------------------------------------------------------------------------------
// Mock host plug-in that will represent LinqPad.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

void Main()
{
	// Create an instance of the resolver specifying the sub directory containing
	// assemblies with plug-ins.  The type resolver will look for all assemblies
	// matching the specified search pattern containing plug-ins.
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new TestTypeResolver(pluginDirectory, 
		"NetFusion.Settings.dll")
	{
		// This indicates that LinqPad, representing the application host, should
		// be scanned for plug-ins.
		LoadAppHostFromAssembly = true
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		// Since LinqPad is the host, add an application host to the
		// container.
		config.AddPlugin<LinqPadHostPlugin>();
	})
	.Build()
	.Start();

	// Use plug-in configured services.  This would normally be 
	// dependency injected into a dependent component such as a
	// WebApi controller.
	var testSettings = AppContainer.Instance.Services.Resolve<TestSettings>();
	testSettings.Dump();
}

// 3_NetFusionSettingsExamples.linq: contains examples for application settings.
public class TestSettings : AppSettings
{
	public TestSettings()
	{
		this.IsInitializationRequired = false;
	}

	public int Value1 { get; set; } = 100;
	public int Value2 { get; set; } = 200;
}