<Query Kind="Program">
  <NuGetReference>NetFusion.Base</NuGetReference>
  <NuGetReference>NetFusion.Bootstrap</NuGetReference>
  <NuGetReference>NetFusion.Common</NuGetReference>
  <NuGetReference>NetFusion.Domain</NuGetReference>
  <NuGetReference>NetFusion.Domain.Messaging</NuGetReference>
  <NuGetReference>NetFusion.Messaging</NuGetReference>
  <NuGetReference>NetFusion.RabbitMQ</NuGetReference>
  <NuGetReference>NetFusion.Settings</NuGetReference>
  <NuGetReference>NetFusion.Test</NuGetReference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Domain.Messaging</Namespace>
  <Namespace>NetFusion.Domain.Modules</Namespace>
  <Namespace>NetFusion.Domain.Scripting</Namespace>
  <Namespace>NetFusion.Messaging</Namespace>
  <Namespace>NetFusion.Messaging.Config</Namespace>
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Core</Namespace>
  <Namespace>NetFusion.RabbitMQ.Exchanges</Namespace>
  <Namespace>NetFusion.Test.Container</Namespace>
  <Namespace>NetFusion.Test.Plugins</Namespace>
  <Namespace>NetFusion.Testing.Logging</Namespace>
  <Namespace>NetFusion.Base</Namespace>
</Query>


void Main()
{
	var brokerSettings = new BrokerSettings
	{
		NumConnectionRetries = 2,
		Connections = new List<BrokerConnectionSettings>
		{
			new BrokerConnectionSettings 
			{
				BrokerName = "TestBroker", 
				HostName = "localhost", 
				UserName="papillon", 
				Password = "bestdog",

				RpcConsumers = new RpcConsumerSettings[] {
					new RpcConsumerSettings {
						RequestQueueKey = "ExampleRpcConsumer",
						RequestQueueName = "RpcMessageQueue",
						CancelRequestAfterMs = 4000
					}
				}
			}
		}
	};

	var resolver = new TestTypeResolver(this.GetType());
	var hostPlugin = new MockAppHostPlugin();
	var corePlugin = new MockCorePlugin();

	corePlugin.UseMessagingPlugin();
	corePlugin.UseRabbitMqPlugin();
	corePlugin.AddPluginType<DomainScriptingModule>();

	resolver.AddPlugin(hostPlugin, corePlugin);
	var container = ContainerSetup.Bootstrap(resolver);
	
	container
		.WithConfig((MessagingConfig config) =>
		{
			// Only required if client will be publishing events.
			config.AddMessagePublisher<RabbitMqMessagePublisher>();
		})
				
		.WithConfig((AutofacRegistrationConfig config) =>
		{
			config.Build = builder =>
			{
				builder.RegisterInstance(brokerSettings);
			};
		});

	// Build and start the container.
	container.Build();
	container.Start();

	var messagingSrv = container.Services.Resolve<IMessagingService>();
	
	var entity = new Car { Make = "Audi", Model = "A6", Year = 2016};
	
	var evt = new ExampleRpcCommand(entity);
	messagingSrv.PublishAsync(evt).Wait();
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

public class ExampleRpcResponse : DomainEvent
{
	public string ResponseTestValue { get; set; }
}
