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
	// Create instance of the HostTypeResolver that is used when configuring a container
	// for unit-testing or for use in LinqPad.  Since a path is not specified, the needed
	// plug-ins must be added manually to the container.
	var typeResolver = new HostTypeResolver();

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