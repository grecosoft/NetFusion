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
</Query>

/// <summary>
/// ---------------------------------------------------------------------------------------------------------
/// WORK-QUEUE EXCHANGE PATTERN
/// ---------------------------------------------------------------------------------------------------------
/// - Used to distribute tasks published to the queue to multiple consumers in a round-robin fashion.
/// - Publisher message RouteKey == Queue Name.
/// - When configuring a work-flow queue, it is defined using the default exchange.
/// - Consumers bind to a work flow queue by using the name assigned to the queue.
/// - Publishers publish messages by specifying the name of queue as the RouteKey.
/// 
/// - The message will be delivered to the queue having the same name as the route key and delivered
///     to bound consumers in a round robin sequence.
/// 
/// - This type of queue is used to distribute time intensive tasks to multiple consumers bound to the queue.
/// - The tasks may take several seconds to complete.  When the consumer is processing the task and fails,
///     another bound consumer should be given the task.  This is achieved by having the client acknowledge
///     the task once its processing has completed.
/// 
/// - There aren't any message timeouts; RabbitMQ will redeliver the message only when the worker connection dies.
///   It's fine even if processing a message takes a very, very long time.
/// 
/// - For this type of queue, it is usually desirable to not loose the task messages if the RabbitMQ server is
///     restarted or would crash.  Two things are required to make sure that messages aren't lost: we need to 
///     mark both the queue and messages as durable.
/// 
/// - If fair dispatch is enabled, RabbitMQ will not dispatch a message to a consumer if there is a pending
///   acknowlegement.This keeps a busy consumer from getting a backlog of messages to process.
/// 
/// - If all the workers are busy, your queue can fill up.  You will want to keep an eye on that, and maybe add
///   more workers, or have some other strategy.
/// </summary>
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
		// Add RabbitMQ Event Publisher to the container.
		 config.AddMessagePublisherType<RabbitMqMessagePublisher>();
	})
	.Build()
	.Start();

	RunPublishToWorkQueueExchange("BMW", "330i", 2015);
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

public void RunPublishToWorkQueueExchange(string make, string model, int year) 
{
	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var domainModel = new Car { Vin = Guid.NewGuid().ToString(), Make = make, Model = model, Year = year };
	var domainEvent = new ExampleWorkQueueEvent(domainModel);
	
	domainEventSrv.PublishAsync(domainEvent);
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container:
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

// -------------------------------------------------------------------------------------
// Model and event message:
// -------------------------------------------------------------------------------------
public class Car
{
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

public class ExampleWorkQueueEvent : DomainEvent
{
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }

	public ExampleWorkQueueEvent() { }

	public ExampleWorkQueueEvent(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.Make = car.Make;
		this.Model = car.Model;
		this.Year = car.Year;

		this.SetRouteKey(car.Make.InSet("VW", "BMW") ? "ProcessSale" : "ProcessService");
	}

	public DateTime CurrentDateTime { get; private set; }
}


// -------------------------------------------------------------------------------------
// Message exchange:
// -------------------------------------------------------------------------------------
public class ExampleWorkQueueExchange : WorkQueueExchange<ExampleWorkQueueEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";

		QueueDeclare("ProcessSale", config =>
		{

		});

		QueueDeclare("ProcessService", config =>
		{

		});
	}
}