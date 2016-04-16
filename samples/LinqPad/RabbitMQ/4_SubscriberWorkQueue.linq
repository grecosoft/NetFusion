<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Bson.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Bson.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.Core.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.Core.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Eventing.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Eventing.dll</Reference>
  <Reference Relative="..\libs\NetFusion.MongoDB.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.MongoDB.dll</Reference>
  <Reference Relative="..\libs\NetFusion.RabbitMQ.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.RabbitMQ.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.Mongo.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.Mongo.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\Newtonsoft.Json.dll</Reference>
  <Reference Relative="..\libs\RabbitMQ.Client.dll">E:\_dev\git\Research\LinqPad\NetFusion\libs\RabbitMQ.Client.dll</Reference>
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
  <Namespace>NetFusion.RabbitMQ.Core</Namespace>
  <Namespace>NetFusion.RabbitMQ.Exchanges</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
</Query>

// **********************************************************************************************
// This is an example of a consumer that subscribes to a work-queue.  Handeling events that
// arrive on a queue is very simular to handeling in-process domain events.  
// **********************************************************************************************

void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new HostTypeResolver(pluginDirectory,
		"NetFusion.Settings.dll",
		"NetFusion.Eventing.dll",
		"NetFusion.RabbitMQ.dll")
	{
		LoadAppHostFromAssembly = true
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		config.AddPlugin<LinqPadHostPlugin>();
	})
	.WithConfig((MessagingConfig config) =>
	{
		// Only requried if client will be publishing events.
		//config.AddEventPublisherType<RabbitMqEventPublisher>();
	})
	.Build()
	.Start();
}

// These settings would normally be stored in a central location.
public class BrokerSettingsInitializer: AppSettingsInitializer<BrokerSettings>
{
	protected override IAppSettings OnConfigure(BrokerSettings settings)
	{
		settings.Connections = new BrokerConnection[] {
			new BrokerConnection { BrokerName = "TestBroker", HostName="LocalHost"}
		};
		
		return settings;
	}
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

[Serializable]
public class WorkQueueEvent : DomainEvent
{
	public DateTime CurrentDateTime { get; set; }
	public string TaskName { get; set; }
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

// -----------------------------------------------------------------------------------------
// Like a normal event consumer, the service implements the IDomainEventConsumer marker
// interface.  In addition, the class needs to be marked with the Broker attribute 
// specifying the broker subscribe.
// -----------------------------------------------------------------------------------------
[Broker("TestBroker")]
public class WorkQueueExchangeService : IMessageConsumer
{
	[JoinQueue("ProcessSale")]
	public void OnProcessSale(WorkQueueEvent workQueueEvent)
	{
		Console.WriteLine($"Handler: OnProcessSale: {workQueueEvent.ToIndentedJson()}");

		workQueueEvent.SetAcknowledged();
	}

	[JoinQueue("ProcessService")]
	public void OnProcessService(WorkQueueEvent workQueueEvent)
	{
		Console.WriteLine($"Handler: OnProcessService: {workQueueEvent.ToIndentedJson()}");

		workQueueEvent.SetAcknowledged();
	}
}