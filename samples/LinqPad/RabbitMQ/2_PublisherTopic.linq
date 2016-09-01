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
</Query>

// ********************************************************************************************
// ---------------------------------------------------------------------------------------------------------
// TOPIC EXCHANGE PATTERN
// ---------------------------------------------------------------------------------------------------------
// -The same as a direct exchange.However, the route key value is a filter and not
//  just a value.
//   
// - The route key used to specify a queue to exchange binding consists of a filter with 
//   '.' delimited values:  A.B.*
// 
// - When a message is posted, the message will only be delivered to the queue if one its
//   binding filter values match the posted route key value.
// ********************************************************************************************
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

	PublishTopicEvent(new Car { Make = "Ford", Model = "Mustang", Year = 2016 });
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

public void PublishTopicEvent(Car car)
{
	var messagingSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new ExampleTopicEvent(car);
	messagingSrv.PublishAsync(evt).Wait();
}

public class Car
{
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

public class ExampleTopicEvent : DomainEvent
{
	public string Vin { get; private set; }
	public string Make { get; private set; }
	public string Model { get; private set; }
	public int Year { get; private set; }

	public ExampleTopicEvent() { }

	public ExampleTopicEvent(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.Vin = car.Vin;
		this.Make = car.Make;
		this.Model = car.Model;
		this.Year = car.Year;

		this.SetRouteKey(car.Make, car.Year, car.Year);
	}

	public DateTime CurrentDateTime { get; private set; }
}

public class ExampleTopicExchange : TopicExchange<ExampleTopicEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "SampleTopicExchange";

		QueueDeclare("Chevy", config =>
		{
			config.RouteKeys = new[] { "Chevy.*.*" };
		});

		QueueDeclare("Chevy-Vette", config =>
		{
			config.RouteKeys = new[] { "Chevy.Vette.*" };
		});

		QueueDeclare("Ford", config =>
		{
			config.RouteKeys = new[] { "Ford.*.*", "Lincoln.*.*" };
		});
	}
}