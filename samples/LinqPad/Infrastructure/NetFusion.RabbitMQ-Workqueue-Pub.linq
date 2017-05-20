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
  <Namespace>NetFusion.Common</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Common.Extensions.Collection</Namespace>
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
</Query>

// **********************************************************************************************
// ---------------------------------------------------------------------------------------------------------
// WORK - QUEUE EXCHANGE PATTERN
// ---------------------------------------------------------------------------------------------------------
// - Used to distribute tasks published to the queue to multiple consumers in a round-robin fashion.
// - Publisher message RouteKey == Queue Name.
// - When configuring a work - flow queue, it is defined using the default exchange.
// - Consumers bind to a work flow queue by using the name assigned to the queue.
// - Publishers publish messages by specifying the name of queue as the RouteKey.
//  
//  - The message will be delivered to the queue having the same name as the route key and delivered
//    to bound consumers in a round robin sequence.
// 
// - This type of queue is used to distribute time intensive tasks to multiple consumers bound to the queue.
// - The tasks may take several seconds to complete.  When the consumer is processing the task and fails,
// 	 another bound consumer should be given the task.  This is achieved by having the client acknowledge
//   the task once its processing has completed.
// 
// - There aren't any message timeouts; RabbitMQ will redeliver the message only when the worker connection dies.
//   It's fine even if processing a message takes a very, very long time.
// 
// - For this type of queue, it is usually desirable to not loose the task messages if the RabbitMQ server is
//   restarted or would crash.  Two things are required to make sure that messages aren't lost: we need to 
//   mark both the queue and messages as durable.
// 
// - If fair dispatch is enabled, RabbitMQ will not dispatch a message to a consumer if there is a pending
//   acknowlegement.This keeps a busy consumer from getting a backlog of messages to process.
// 
// - If all the workers are busy, your queue can fill up.You will want to keep an eye on that, and maybe add
//   more workers, or have some other strategy.
// **********************************************************************************************

void Main()
{
	var brokerSettings = new BrokerSettings
	{
		NumConnectionRetries = 2,
		Connections = new List<BrokerConnectionSettings>
		{
			new BrokerConnectionSettings { BrokerName = "TestBroker", HostName = "localhost", UserName="papillon", Password = "bestdog" }
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
	
	var entity = new Car { Make = "Audi", Model = "A4", Year = 2016 };
	
	var evt = new ExampleWorkQueueEvent(entity);
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
public class ExampleWorkQueueEvent : DomainEvent
{
	public string Make { get; private set; }
	public string Model { get; private set; }
	public int Year { get; private set; }

	public ExampleWorkQueueEvent() { }

	public ExampleWorkQueueEvent(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.Make = car.Make;
		this.Model = car.Model;
		this.Year = car.Year;

		this.SetRouteKey(car.Make.InSet("VW", "BMW") ? "Process_Sale" : "Process_Service");
	}

	public DateTime CurrentDateTime { get; private set; }
}

public class ExampleWorkQueueExchange : WorkQueueExchange<ExampleWorkQueueEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";

		QueueDeclare("process_sale");
		QueueDeclare("PROCESS_SERVICE");
	}
}
