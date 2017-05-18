<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Domain.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Domain.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Messaging.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Messaging.dll</Reference>
  <Reference Relative="..\libs\NetFusion.RabbitMQ.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.RabbitMQ.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Newtonsoft.Json.dll</Reference>
  <Reference Relative="..\libs\RabbitMQ.Client.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\RabbitMQ.Client.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Messaging</Namespace>
  <Namespace>NetFusion.Messaging.Config</Namespace>
  <Namespace>NetFusion.Messaging.Rules</Namespace>
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.RabbitMQ.Core</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
</Query>

// **********************************************************************************************
// This is an example of a consumer that subscribes to a direct queue.  Handeling events that
// arrive on a queue is very simular to handeling in-process domain events.  
// **********************************************************************************************

void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new TestTypeResolver(pluginDirectory,
		"NetFusion.Settings.dll",
		"NetFusion.Domain.dll",
		"NetFusion.Messaging.dll",
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

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container:
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

// -------------------------------------------------------------------------------------
// Boker Configuration Settings:
// -------------------------------------------------------------------------------------
public class BrokerSettingsInitializer : AppSettingsInitializer<BrokerSettings>
{
	protected override IAppSettings OnConfigure(BrokerSettings settings)
	{
		settings.Connections = new BrokerConnection[] {
			new BrokerConnection { BrokerName = "TestBroker", HostName="LocalHost"}
		};

		return settings;
	}
}

[Serializable]
public class ExampleTopicEvent : DomainEvent
{
	public string Make { get; private set; }
	public string Model { get; private set; }
	public int Year { get; private set; }
	public string Color { get; private set; }

}

[Broker("TestBroker")]
public class ExampleTopicService : IMessageConsumer
{
	// This method will join the VW-GTI queue defined on the SampleTopicExchange
	// exchange.  Since it is joining an existing queue, it will join any other
	// enlisted subscribers and be called round-robin.  
	[JoinQueue("VW-GTI", "SampleTopicExchange")]
	public void OnVwGti(ExampleTopicEvent topicEvt)
	{
		Console.WriteLine($"Handler: OnVwGti: {topicEvt.ToIndentedJson()}");

		topicEvt.SetAcknowledged();
	}

	// This event handler will join the VW-BLACK queue defined on the
	// SampleTopicExchange.  This handler is like the prior one, but the
	// associated queue has a more specific route-key pattern.  Both this
	// handler and the prior one will both be called since this handler 
	// has a more specific pattern to include the color of the car.
	[JoinQueue("VW-BLACK", "SampleTopicExchange")]
	public void OnBlackVw(ExampleTopicEvent topicEvt)
	{
		Console.WriteLine($"Handler: OnBlackVw: {topicEvt.ToIndentedJson()}");

		topicEvt.SetAcknowledged();
	}

	// This event handler creates a new queue on SampleTopicExchange matching 
	// a specific route key.  This will not join an existing queue but will 
	// create a new queue specific for this host application.  If you start
	// another instance of this application, both will create their own queues
	// and therefore both be called.  This is more of a notification queue
	// since the message does not require an acknowledgment and the queue
	// will be deleted when the host application disconnects.
	[AddQueue("SampleTopicExchange", RouteKey = "VW.JETTA.*.*",
		IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
	public void OnVwJetta(ExampleTopicEvent topicEvt)
	{
		Console.WriteLine($"Handler: OnVwJetta: {topicEvt.ToIndentedJson()}");
	}

	// This adds a queue with a more specific pattern.  This queue is configured 
	// to require message acknowlegement.  Also auto-delete is not set so the queue
	// will remain after the host application disconnects.  This would be the setup
	// for important messages that are specific to a given application host.
	[AddQueue("VW-PASSAT-SILVER", "SampleTopicExchange", RouteKey = "VW.PASSAT.*.SILVER",
		IsAutoDelete = false, IsNoAck = false, IsExclusive = false)]
	public void OnSilverVwPassat(ExampleTopicEvent topicEvt)
	{
		Console.WriteLine($"Handler: OnSilverVwPassat: {topicEvt.ToIndentedJson()}");

		topicEvt.SetAcknowledged();
	}

	// This example joins a queue defined by the publisher where the route-key
	// values are stored in the configuration and not specified in code.
	[JoinQueue("AUDI", "SampleTopicExchange")]
	public void OnAudi(ExampleTopicEvent topicEvt)
	{
		Console.WriteLine($"Handler: OnAudi: {topicEvt.ToIndentedJson()}");

		topicEvt.SetAcknowledged();
	}
}