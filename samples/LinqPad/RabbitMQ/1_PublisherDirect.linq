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

/// ********************************************************************************************
/// Direct Exchange
/// ********************************************************************************************
/// - The direct exchange uses the route key to determine which queues should receive the 
///   message.  When a queue is bound to an exchange, it can specify a route key value.
///     
/// - When messages are published to a direct exchange, the publisher specifies a route key 
///   value.  The exchange will deliver the message to all queues that have a binding with 
///   the specified route key value.
///     
/// - A given queue to be bound more than once to an exchange - each binding using a different 
///   route key.  If a message's routing key does not match any of the queue bindings, the message
///   is discarded.
/// 
/// - It is perfectly legal to bind multiple queues with the same binding key.  In that case, the 
///   direct exchange will behave like fanout and will broadcast the message to all the matching
///   queues.
/// </summary>
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
	var directEvt = new DirectEvent 
	{ 
		CurrentDateTime = DateTime.Now, 
		Vin=Guid.NewGuid().ToString(), 
		Make = make,
		Model = model,
		Year = year
	};

	directEvt.SetRouteKey(year);

	if (year < 2015)
	{
		directEvt.SetRouteKey("UsedModel");
	}

	domainEventSrv.PublishAsync(directEvt).Wait();
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

[Serializable]
public class DirectEvent : DomainEvent
{
	public DateTime CurrentDateTime { get; set; }
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

public class DirectExchange : DirectExchange<DirectEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "SampleDirectExchange";

		QueueDeclare("2015-2016-Cars", config =>
		{
			config.RouteKeys = new[] { "2015", "2016" };
		});

		QueueDeclare("UsedCars", config =>
		{
			config.RouteKeys = new[] { "UsedModel" };
		});
	}
}