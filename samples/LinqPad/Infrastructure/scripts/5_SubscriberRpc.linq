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
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>NetFusion.RabbitMQ.Serialization</Namespace>
  <Namespace>NetFusion.RabbitMQ.Exchanges</Namespace>
</Query>

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

public class ExampleRpcExchange : RpcExchange
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";

		QueueDeclare("RpcMessageQueue", config =>
		{

		});
	}
}

public class Car
{
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
}

[RpcCommand("TestBroker", "ExampleRpcConsumer",
	   nameof(ExampleRpcCommand), ContentType = SerializerTypes.Json)]
[Serializable]
public class ExampleRpcCommand : Command<ExampleRpcResponse>
{
	public DateTime CurrentDateTime { get; private set; }
	public string InputValue { get; private set; }
	public int DelayInMs { get; private set; }

	public ExampleRpcCommand()
	{

	}

	public ExampleRpcCommand(Car car)
	{
		var rand = new Random();
		this.DelayInMs = rand.Next(0, 500);

		this.CurrentDateTime = DateTime.UtcNow;
		this.InputValue = $"{car.Make + car.Model}";
	}
}

[Serializable]
public class ExampleRpcResponse : DomainEvent
{
	public string ResponseTestValue { get; set; }
}

public class ExampleRpcService : IMessageConsumer
    {
        [InProcessHandler]
        public async Task<ExampleRpcResponse> OnRpcMessage(ExampleRpcCommand rpcCommand)
        {
            Console.WriteLine($"Delay: {rpcCommand.DelayInMs} TestValue: {rpcCommand.InputValue}");

            await Task.Run(() =>
            {
                Thread.Sleep(rpcCommand.DelayInMs);
				//  throw new InvalidOperationException("TEST");
			});

		return new ExampleRpcResponse
		{
			ResponseTestValue = rpcCommand.InputValue + " " + DateTime.UtcNow
		};
	}
}