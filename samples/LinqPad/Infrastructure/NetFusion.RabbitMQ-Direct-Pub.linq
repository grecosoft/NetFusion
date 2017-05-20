<Query Kind="Program">
  <NuGetReference>NetFusion.Base</NuGetReference>
  <NuGetReference>NetFusion.Bootstrap</NuGetReference>
  <NuGetReference>NetFusion.Common</NuGetReference>
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
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.RabbitMQ.Core</Namespace>
  <Namespace>NetFusion.RabbitMQ.Exchanges</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Test.Container</Namespace>
  <Namespace>NetFusion.Test.Plugins</Namespace>
  <Namespace>NetFusion.Testing.Logging</Namespace>
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
	
	var entity = new Car
	{
		Make = "AUDI",
		Model = "S8",
		Year = 1973,
		Color = "Red"
	};
	
	var evt = new ExampleDirectEvent(entity);
	messagingSrv.PublishAsync(evt).Wait();
	
}

public class Car
{
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
}

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

		this.SetRouteKey(car.Make); 
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