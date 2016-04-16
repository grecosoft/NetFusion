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

//**********************************************
// Publish/Subscribe(Fan-out Exchange)
//**********************************************
// - This scenario uses exchange which is responsible for determining and delivering messages to queues.
// - An exchange of type Fan-out will broadcast a message to all queues defined on the exchange.
// - This type of configuration is often used when the message needs to be sent to many receivers.
// - This configuration usually does not require the message to be acknowledged by the consumers.
// - This setup is achieved by having each receiver define an queue that is bound to by the consumer and
//   automatically deleted once the consumer disconnects.
//
// - Route keys are not used by this type of exchange.
//
// - Giving a queue a name is important when you want to share the queue between producers and consumers.
//     But in this case, the publisher usually does not know about the consumers.  In this case a temporary
//     named queue is best.  Also, the consumer is usually not interested in old messages, just current ones.
//
// - Firstly, whenever we connect to Rabbit we need a fresh, empty queue. To do this we could create a queue
//   with a random name, or, even better - let the server choose a random queue name for us.Secondly, once we
//   disconnect the consumer the queue should be automatically deleted.
//
// - The messages will be lost if no queue is bound to the exchange yet.For most publish/subscribe scenarios
//   this is what we would want.   
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
		// Add RabbitMQ Event Publisher to the container.
		 config.AddMessagePublisherType<RabbitMqMessagePublisher>();
	})
	.Build()
	.Start();

	RunPublishToDirectExchange("BMW", "330i", 2015);
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

public void RunPublishToDirectExchange(string make, string model, int year) 
{
	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var fanoutEvt = new FanoutEvent
	{ 
		CurrentDateTime = DateTime.Now, 
		Vin=Guid.NewGuid().ToString(), 
		Make = make,
		Model = model,
		Year = year
	};
	
	fanoutEvt.SetRouteKey(make);
	domainEventSrv.PublishAsync(fanoutEvt).Wait();
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

[Serializable]
public class FanoutEvent : DomainEvent
{
	public DateTime CurrentDateTime { get; set; }
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

public class GermanCarExchange : FanoutExchange<FanoutEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleFanoutExchange->GermanCars";
        }

        protected override bool Matches(FanoutEvent domainEvent)
	{
		return domainEvent.Make.InSet("VW", "Audi", "BMW");
	}

}

public class AmericanCarExchange : FanoutExchange<FanoutEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "SampleFanoutExchange->AmericanCars";
	}

	protected override bool Matches(FanoutEvent domainEvent)
	{
		return domainEvent.Make.InSet("Ford", "GMC", "Chevy");
	}
}