<Query Kind="Program">
  <Reference Relative="libs\Autofac.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Autofac.dll</Reference>
  <Reference Relative="libs\MongoDB.Bson.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Bson.dll</Reference>
  <Reference Relative="libs\MongoDB.Driver.Core.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.Core.dll</Reference>
  <Reference Relative="libs\MongoDB.Driver.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.dll</Reference>
  <Reference Relative="libs\NetFusion.Bootstrap.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="libs\NetFusion.Common.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="libs\NetFusion.Eventing.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Eventing.dll</Reference>
  <Reference Relative="libs\NetFusion.MongoDB.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.MongoDB.dll</Reference>
  <Reference Relative="libs\NetFusion.RabbitMQ.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.RabbitMQ.dll</Reference>
  <Reference Relative="libs\NetFusion.Settings.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="libs\NetFusion.Settings.Mongo.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.Mongo.dll</Reference>
  <Reference Relative="libs\Newtonsoft.Json.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Newtonsoft.Json.dll</Reference>
  <Reference Relative="libs\RabbitMQ.Client.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\RabbitMQ.Client.dll</Reference>
  <Reference Relative="libs\Samples.Domain.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Samples.Domain.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Eventing</Namespace>
  <Namespace>NetFusion.Eventing.Config</Namespace>
  <Namespace>NetFusion.MongoDB</Namespace>
  <Namespace>NetFusion.MongoDB.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>Samples.Domain.RabbitMQ.Events</Namespace>
</Query>



void Main()
{
	var dumpContainerConfig = false;
	var waitForInput = false;

	// ********************************************************************************************
	// ** CONTAINER INITIALIZATION
	// ********************************************************************************************

	// Create instance of type resolver that will look in a specific directory.  Also, specify
	// LoadAppHostFromAssembly to be True.  This will associate all types contained within the 
	// LinqPad editor as being plug-in types of LinqPadHostPlugin.
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "libs");

	var typeResolver = new HostTypeResolver(pluginDirectory, "*.dll") 
	{
		LoadAppHostFromAssembly = true 
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config => {
	
		config.AddPlugin<LinqPadHostPlugin>();
		
		config.AddPlugin<TestCorePlugin>()
			.AddPluginType<TestCoreModule>()
			.AddPluginType<ISomePluginDefinedType>();
			
		config.AddPlugin<TestAppComponentPlugin>()
			.AddPluginType<PluginBasedDerivedType>();

	})
	.Build()
	.Start();
	
	if (dumpContainerConfig)
	{
		AppContainer.Instance.Log.Dump();
	}
	
	SettingsExample();
	InsertDocumentIntoMongoExample();
	//PlublishInProcessDomainEventExample();
	
	
	
	if (waitForInput)
	{
		Console.ReadLine();
	}
}

// This class represents the application's host.  Since this type is defined
// in LinqPad and LoadAppHostFromAssembly is specified as True above, the
// HostTypeResolver will load all types defined within LinqPad and associate
// them with the plug-in.
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}


// --------------------------------------------------------------------------
// The following shows examples of the application settings Plug-in.
// --------------------------------------------------------------------------

private void SettingsExample()
{
	var settings = AppContainer.Instance.Services.Resolve<TestSettings>();
	settings.Dump();
	
	var settings2 = AppContainer.Instance.Services.Resolve<TestSettings2>();
	settings2.Dump();
}

// Settings POCO for which no IAppSettingsInitalizer needs to be
// defined.  In this case, the values specified in code directly
// on the settings class will be the settings values.
public class TestSettings : AppSettings
{
	public TestSettings()
	{
		this.IsInitializationRequired = false;
	}

	public string TestValue1 => "Value1";
	public int TestValue2 => 100;
}

// This setting must have a defined initializer within the application
// host or within an application component.
public class TestSettings2 : AppSettings
{
	public string TestValue1 { get; set; } = "Value2";
	public int TestValue2 { get; set; } = 200;
}

public class TestSettingsInitializer : AppSettingsInitializer<TestSettings2>
{
	protected override IAppSettings OnConfigure(TestSettings2 settings)
	{
		settings.TestValue1 = "Set in initializer";
		settings.TestValue2 += 1000;
		return settings;
	}
}



// --------------------------------------------------------------------------
// The following shows examples of the MongoDB plug-in.
// --------------------------------------------------------------------------

private void InsertDocumentIntoMongoExample()
{
	var context = AppContainer.Instance.Services.Resolve<IMongoDbClient<LinqPadTestDb>>();
	var testDocColl = context.GetCollection<TestDocument>();

	var testDoc = new TestDocument
	{
		ValueOne = "test value one",
		ValueTwo = "test value two"
	};

	testDocColl.InsertOneAsync(testDoc).Wait();
	testDoc.TestDocId.Dump();
}

public class LinqPadTestDb : MongoSettings
{
	public LinqPadTestDb()
	{
		this.IsInitializationRequired = false;
		this.DatabaseName = "LinkPadTestDb";
		this.MongoUrl = "mongodb://localhost:27017";
	}
}

public class TestDocument
{
	public string TestDocId { get; set; }
	public string ValueOne { get; set; }
	public string ValueTwo { get; set; }
}

public class PlantMap : EntityClassMap<TestDocument>
{
	public PlantMap()
	{
		this.CollectionName = "LinqPad.TestDoc";

		this.AutoMap();
		MapStringObjectIdProperty(p => p.TestDocId);
	}
}

















private void PlublishInProcessDomainEventExample()
{
	var testEvt = new TestEvent { Value1 = "Test-Value" };

	var domainEventSrv = AppContainer.Instance.Services.Resolve<IDomainEventService>();
	domainEventSrv.PublishAsync(testEvt).Wait();
}





public class TestCorePlugin : MockPlugin,
	ICorePluginManifest
{
}

public class TestAppComponentPlugin : MockPlugin,
	IAppComponentPluginManifest
{
}

public class TestCoreModule : PluginModule
{
	public IEnumerable<ISomePluginDefinedType> SomeDefinedInstances { get; set;}
}

public interface ISomePluginDefinedType : IKnownPluginType
{
	int GetValueOne();
	string GetValueTwo();
}

public class PluginBasedDerivedType : ISomePluginDefinedType
{
	public int GetValueOne()
	{
		return 110;
	}

	public string GetValueTwo()
	{
	return "String Value";
	}
}





public class TestEvent : DomainEvent
{
	public string Value1 {get; set;}
}

[Broker("TestBroker")]
public class TestService : IDomainEventConsumer {

	public void OnTestEvent(TestEvent evt)
	{
		evt.Dump();
	}

	[JoinQueue("Chevy-Vette", "SampleTopicExchange")]
	public void OnChevyVette(TopicEvent topicEvt)
	{
		topicEvt.Dump();

		topicEvt.SetAcknowledged();
	}

	[JoinQueue("Ford", "SampleTopicExchange")]
	public void OnFord(TopicEvent topicEvt)
	{
		topicEvt.Dump();
		topicEvt.SetAcknowledged();
	}
}











// Move following into host assembly...




public class BrokerSettingsInitializer : AppSettingsInitializer<BrokerSettings>
{
	protected override IAppSettings OnConfigure(BrokerSettings settings)
	{
		return new BrokerSettings
		{
			Connections = new BrokerConnection[]
			{
				new BrokerConnection { BrokerName = "TestBroker", HostName = "LocalHost" }
			}
		};
	}
}