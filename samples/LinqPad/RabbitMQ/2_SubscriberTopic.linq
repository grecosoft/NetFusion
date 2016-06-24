<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
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
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.RabbitMQ.Core</Namespace>
  <Namespace>NetFusion.RabbitMQ.Exchanges</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
  <Namespace>NetFusion.Messaging.Config</Namespace>
  <Namespace>NetFusion.Messaging</Namespace>
  <Namespace>NetFusion.Messaging.Rules</Namespace>
</Query>

// **********************************************************************************************
// This is an example of a consumer that subscribes to a direct queue.  Handeling events that
// arrive on a queue is very simular to handeling in-process domain events.  
// **********************************************************************************************

void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new HostTypeResolver(pluginDirectory,
		"NetFusion.Settings.dll",
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

public class ExampleTopicEvent : DomainEvent
{
	public DateTime CurrentDateTime { get; set; }
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

public class ShortMakeRule : MessageDispatchRule<ExampleTopicEvent>
{
	protected override bool IsMatch(ExampleTopicEvent message)
	{
		return message.Make.Length == 3;
	}
}

// -----------------------------------------------------------------------------------------
// Like a normal event consumer, the service implements the IDomainEventConsumer marker
// interface.  In addition, the class needs to be marked with the Broker attribute 
// specifying the broker subscribe.
// -----------------------------------------------------------------------------------------
[Broker("TestBroker")]
public class ExampleTopicService : IMessageConsumer
{
    // This method will join the Chevy queue defined on the SampleTopicExchange
    // exchange.  Since it is joining an existing queue, it will join any other
    // enlisted subscribers and be called round-robin.  
    [JoinQueue("Chevy", "SampleTopicExchange")]
    public void OnChevy(ExampleTopicEvent topicEvt)
    {
        Console.WriteLine($"Handler: OnChevy: {topicEvt.ToIndentedJson()}");

        topicEvt.SetAcknowledged();
    }

    // This event handler will join the Chevy-Vette queue defined on the
    // SampleTopicExchange.  This handler is like the prior one, but the
    // associated queue has a more specific route-key pattern.  Both this
    // handler and the prior one will both be called since this handler 
    // has a more specific pattern to include the model of the car.
    [JoinQueue("Chevy-Vette", "SampleTopicExchange")]
    public void OnChevyVette(ExampleTopicEvent topicEvt)
    {
        Console.WriteLine($"Handler: OnChevyVette: {topicEvt.ToIndentedJson()}");

        topicEvt.SetAcknowledged();
    }

    // This event handler joins the Ford queue defined on the same
    // exchange as the prior two event handlers.
    [JoinQueue("Ford", "SampleTopicExchange")]
    public void OnFord(ExampleTopicEvent topicEvt)
    {
        Console.WriteLine($"Handler: OnFord: {topicEvt.ToIndentedJson()}");

        topicEvt.SetAcknowledged();
    }

    // This event handler creates a new queue on SampleTopicExchange 
    // matching any route key.  However, this event handler has a 
    // dispatch-role specified to only be called if the Make of the
    // car is <= three characters.  The event is still delivered but
    // just not passed to this handler.  If there are a large number
    // of events, it is best to create a dedicated queue on the 
    // exchange.
    [ApplyDispatchRule(typeof(ShortMakeRule))]
    [AddQueue("SampleTopicExchange", RouteKey = "#",
        IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
    public void OnSortMakeName(ExampleTopicEvent topicEvt)
    {
        Console.WriteLine($"Handler: OnSortMakeNam: {topicEvt.ToIndentedJson()}");
    }

	// This adds a queue with a more specific pattern.  Since it 
	// creating a new queue, it will be called in addition to the
	// event handler that is specified for the Ford queue.
	[AddQueue("SampleTopicExchange", RouteKey = "Ford.Mustang.*",
		IsAutoDelete = true, IsNoAck = true, IsExclusive = true)]
	public void OnFordMustang(ExampleTopicEvent topicEvt)
	{
		Console.WriteLine($"Handler: OnFordMustang: {topicEvt.ToIndentedJson()}");
	}
}