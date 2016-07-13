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

// **************************************************************************************
// --------------------------------------------------------------------------------------
// RPC EXCHANGE PATTERN
// --------------------------------------------------------------------------------------
// The message that is published for this exchange pattern must be a command.With this 
// pattern, the publisher publishes the command and the subscriber returns a response.
// ***************************************************************************************
void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new TestTypeResolver(pluginDirectory,
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

	PublishRPCEvent(new Car { Make = "Audi", Model = "A6", Year = 2016}).Dump();
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

public ExampleRpcResponse PublishRPCEvent(Car car)
{
	var messagingSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new ExampleRpcCommand(car);
	return messagingSrv.PublishAsync(evt).Result;
}

public class Car
{
	public string Vin { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
}

public class ExampleRpcCommand : Command<ExampleRpcResponse>
{
	public DateTime CurrentDateTime { get; private set; }
	public string InputValue { get; private set; }

	public ExampleRpcCommand()
	{
		this.SetRouteKey("Hello");
	}

	public ExampleRpcCommand(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.InputValue = $"{car.Make + car.Model}";
	}
}

public class ExampleRpcResponse : DomainEvent
{
	public string Comment { get; set; }
}

public class ExampleRpcExchange : DirectExchange<ExampleRpcCommand>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "SampleRpcExchange";
		SetReturnType<ExampleRpcResponse>();

		QueueDeclare("QueueWithConsumerResponse", config =>
		{
			config.RouteKeys = new[] { "Hello" };
		});
	}
}