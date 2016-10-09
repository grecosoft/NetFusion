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
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.RabbitMQ.Core</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
  <Namespace>NetFusion.RabbitMQ.Exchanges</Namespace>
  <Namespace>NetFusion.Domain.Scripting</Namespace>
</Query>

// *************************************************************************************
// ---------------------------------------------------------------------------------------------------------
// DIRECT EXCHANGE PATTERN
// -------------------------------------------------------------------------------------------------------- -
// -The direct exchange uses the route key to determine which queues should receive the
//  message.When a queue is bound to an exchange, it can specify a route key value.
//     
// - When messages are published to a direct exchange, the publisher specifies a route key
//   value.The exchange will deliver the message to all queues that have a binding with
//   the specified route key value.
//     
// - A given queue to be bound more than once to an exchange - each binding using a different
//   route key.If a message's routing key does not match any of the queue bindings, the message
//   is discarded.
// 
// - It is perfectly legal to bind multiple queues with the same binding key.In that case, the
//   direct exchange will behave like fanout and will broadcast the message to all the matching
//   queues.
// *************************************************************************************
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
		// Add RabbitMQ Event Publisher to the container.
		 config.AddMessagePublisherType<RabbitMqMessagePublisher>();
	})
	.Build()
	.Start();

	PublishDirectEvent(new Car {
		Make = "AUDI",
		Model = "S8",
		Year = 1973,
		Color = "Red"
	});
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

public void PublishDirectEvent(Car car)
{
	var messagingSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new ExampleDirectEvent(car);
	messagingSrv.PublishAsync(evt).Wait();
}

public class Car
{
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
}

[Serializable]
public class ExampleDirectEvent : DomainEvent
{
	public string Make { get; private set; }
	public string Model { get; private set; }
	public int Year { get; private set; }
	public string Color { get; private set; }

	public ExampleDirectEvent() { }

	public ExampleDirectEvent(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.Make = car.Make;
		this.Model = car.Model;
		this.Year = car.Year;
		this.Color = car.Color;

		this.SetRouteKey(car.Make); // TODO:  move this out of the RabbitMQ project??? So client don't need ref?
	}

	public DateTime CurrentDateTime { get; private set; }
}

[ApplyScriptPredicate("ClassicCarCriteria", variableName: "IsClassic")]
public class ExampleDirectExchange : DirectExchange<ExampleDirectEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "SampleDirectExchange";

		QueueDeclare("GENERAL-MOTORS", config =>
		{
			config.RouteKeys = new[] { "CHEVY", "Buick", "GMC", "CADILLAC" };
		});

		QueueDeclare("VOLKSWAGEN", config =>
		{
			config.RouteKeys = new[] { "VW", "Audi", "PORSCHE", "BENTLEY", "LAMBORGHINI" };
		});
	}
}