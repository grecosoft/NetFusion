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
</Query>

// *******************************************************************************************************
// ---------------------------------------------------------------------------------------------------------
// FANOUT EXCHANGE PATTERN
// -------------------------------------------------------------------------------------------------------- -
// - This scenario uses exchange which is responsible for determining and delivering messages to queues.
// - An exchange of type Fan -out will broadcast a message to all queues defined on the exchange.
// - This type of configuration is often used when the message needs to be sent to many receivers.
// - This configuration usually does not require the message to be acknowledged by the consumers.
// - This setup is achieved by having each receiver define an queue that is bound to by the consumer and
//   automatically deleted once the consumer disconnects.
// 
// - Route keys are not used by this type of exchange.
// 
// - Giving a queue a name is important when you want to share the queue between producers and consumers.
//   But in this case, the publisher usually does not know about the consumers.In this case a temporary
//     named queue is best.Also, the consumer is usually not interested in old messages, just current ones.
// 
// - Firstly, whenever we connect to Rabbit we need a fresh, empty queue.To do this we could create a queue
//   with a random name, or, even better - let the server choose a random queue name for us.Secondly, once
//   we disconnect the consumer the queue should be automatically deleted.
// 
// - The messages will be lost if no queue is bound to the exchange yet.For most publish/subscribe scenarios
//   this is what we would want. 
// 
// *******************************************************************************************************
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
	
	var entity = new Car { Make = "BMW", Model = "M3", Year = 2016 };
	
	var evt = new ExampleFanoutEvent(entity);
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
public class ExampleFanoutEvent : DomainEvent
{
	public string Make { get; private set; }
	public string Model { get; private set; }
	public int Year { get; private set; }
	public string Color { get; private set; }

	public ExampleFanoutEvent() { }

	public ExampleFanoutEvent(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.Make = car.Make;
		this.Model = car.Model;
		this.Year = car.Year;
		this.Color = car.Color;
	}

	public DateTime CurrentDateTime { get; private set; }
}

[ApplyScriptPredicate("HighImportanceCriteria", variableName: "IsHighImportance")]
public class HighImportanceExchange : FanoutExchange<ExampleFanoutEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "HighImportanceExchange";
	}
}

public class LowImportanceExchange : FanoutExchange<ExampleFanoutEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "LowImportanceExchange";
	}

	protected override bool Matches(ExampleFanoutEvent message)
	{
		return message.Make.Equals("Toyota", System.StringComparison.OrdinalIgnoreCase) && message.Year > 2014;
	}
}