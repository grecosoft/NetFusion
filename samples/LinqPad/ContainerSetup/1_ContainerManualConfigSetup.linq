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

// *****************************************************************************************
// This example configures the container manually without automatically discovering plug-ins 
// by searching a specified directory.  This allows for quick testing and development of new
// plug-ins directly in LinqPad using the exact code that is executed when hosted in an actual
// host such as WebApi.  See 2_ContainerAutoConfigSetup.linq for examples of automatically 
// searching assemblies and LinqPad for plug-ins. 
// *****************************************************************************************

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container to represent LinqPad.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

} 

void Main()
{
	// Create instance of the TestTypeResolver that is used when configuring a container
	// for unit-testing or for use in LinqPad.  Since a path is not specified, the needed
	// plug-ins must be added manually to the container.  Each plug-in has an extension 
	// method that will add a manifest and its corresponding types.
	var typeResolver = new TestTypeResolver();

	MockPlugin hostPlugin = null;

	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		// Add needed plugins. 
		config.AddSettingsPlugin();
		
		// Since LinqPad is the host, add an application host to the
		// container.
		hostPlugin = config.AddPlugin<LinqPadHostPlugin>();
	});

	// Add plug-in specific types to the host plug-in that are
	// defined within LinqPad.
	hostPlugin.AddPluginType<TestSettings>();
	
	// Build and start the container:
	AppContainer.Instance
		.Build()
		.Start();

	// Use plug-in configured services.  This would normally be 
	// dependency injected into a dependent component such as a
	// WebApi controller.
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