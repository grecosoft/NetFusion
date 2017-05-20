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
	var entity = new Car { Make = "VW", Model = "GTI", Year = 2016 };
	var evt = new ExampleTopicEvent(entity);
	messagingSrv.PublishAsync(evt).Wait();	
}

public class Car
{
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
}

public class ExampleTopicEvent : DomainEvent
{
	public string Make { get; private set; }
	public string Model { get; private set; }
	public int Year { get; private set; }
	public string Color { get; private set; }

	public ExampleTopicEvent() { }

	public ExampleTopicEvent(Car car)
	{
		this.CurrentDateTime = DateTime.UtcNow;
		this.Make = car.Make;
		this.Model = car.Model;
		this.Year = car.Year;
		this.Color = car.Color;

		this.SetRouteKey(car.Make, car.Model, car.Year, car.Color);
	}

	public DateTime CurrentDateTime { get; private set; }
}

public class ExampleTopicExchange : TopicExchange<ExampleTopicEvent>
{
	protected override void OnDeclareExchange()
	{
		Settings.BrokerName = "TestBroker";
		Settings.ExchangeName = "SampleTopicExchange";

		QueueDeclare("VW-GTI", config =>
		{
			config.RouteKeys = new[] { "VW.GTI.*.*" };
		});

		QueueDeclare("VW-BLACK", config =>
		{
			config.RouteKeys = new[] { "VW.*.*.BLACK" };
		});

		QueueDeclare("AUDI", config =>
		{
			config.RouteKeys = new[] { "Audi.*.*.*" };
		});
	}
}