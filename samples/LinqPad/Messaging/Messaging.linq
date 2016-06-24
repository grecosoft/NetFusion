<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">C:\Users\greco\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">C:\Users\greco\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">C:\Users\greco\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Messaging.dll">C:\Users\greco\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Messaging.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">C:\Users\greco\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">C:\Users\greco\_dev\git\NetFusion\samples\LinqPad\libs\Newtonsoft.Json.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>NetFusion.Messaging</Namespace>
  <Namespace>NetFusion.Messaging.Rules</Namespace>
</Query>

// *****************************************************************************************
// This query shows examples of using the Eventing Plug-in for publishing local domain
// events and commands.  By default, the InProcessEventDispatcher is configured which
// implements the dispatching of messages to handlers defined within the local process.
// The eventing plug-in defines an extension point where other publisher implementations
// can be called to handle the dispatching of messages.  For publishing messages to 
// RabbitMQ, see example query:  6_NetFusionRabbitMqExamples.
// *****************************************************************************************
void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new HostTypeResolver(pluginDirectory,
		"NetFusion.Settings.dll",
		"NetFusion.Messaging.dll")
	{
		LoadAppHostFromAssembly = true
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		config.AddPlugin<LinqPadHostPlugin>();
	})
	.Build()
	.Start();

	// Execute the examples:
	RunPublishDomainEventSyncExample();
	RunPublishDomainEventAsyncExample();
	RunPublishCommandEventAsyncExample();
	RunDispatchRuleExample();
	RunBaseEventHandlerExample();
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

// *****************************************************************************************
// A domain-event can be published to one or more event handlers.  Event handlers can be
// synchronous or asynchronous.  This example shows publishing a domain event to two
// synchronous handlers.  Note:  The event handler definition determines if the handler
// is invoked synchronously or asychronously and not the domain event.  The domain event
// is just a simple POCO.  Since event handlers can be either synchronous or asynchronous,
// the method to invoke this is an asynchronous method.
// *****************************************************************************************
public void RunPublishDomainEventSyncExample()
{
	nameof(RunPublishDomainEventSyncExample).Dump();

	// Events are published by using the IDomainEventService:
	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();

	var evt = new TestDomainEvent1("Value One", "Value Two");
	domainEventSrv.PublishAsync(evt).Wait();
	
	var evt2 = new TestDomainEvent2(100, 200);
	domainEventSrv.PublishAsync(evt2).Wait();
}

// -------------------------------------------------------------------------------------
// Example of a domain-event.
// -------------------------------------------------------------------------------------
public class TestDomainEvent1 : DomainEvent
{
	public TestDomainEvent1(string value1, string value2)
	{
		this.Value1 = value1;
		this.Value2 = value2;
	}

	public string Value1 { get; }
	public string Value2 { get; }
}

public class TestDomainEvent2 : DomainEvent
{
	public int Value1 { get; }
	public int Value2 { get; }
	
	public TestDomainEvent2(int value1, int value2)
	{
		this.Value1 = value1;
		this.Value2 = value2;
	}
}

// ---------------------------------------------------------------------------------------
// Components that handle events are identified by implementing the IDomainEventConsumer
// marker interface and defining a method taking the event type to handle.  A given 
// component can handle one or more events.
// --------------------------------------------------------------------------------------
public class TestHandlerSync1 : IMessageConsumer
{
	public void OnEvent(TestDomainEvent1 evt)
	{
		nameof(TestHandlerSync1).Dump();
		evt.Dump();
	}

	public void OnEvent(TestDomainEvent2 evt)
	{
		nameof(TestHandlerSync1).Dump();
		evt.Dump();
	}
}

public class TestHandlerSync2 : IMessageConsumer
{
	public void OnEvent(TestDomainEvent1 evt)
	{
		nameof(TestHandlerSync2).Dump();
		evt.Dump();
	}

	public void OnEvent(TestDomainEvent3 evt)
	{
		nameof(TestHandlerSync2).Dump();
		evt.Dump();
	}
}


// *****************************************************************************************
// Domain event handlers can also be asynchronous or a combination of both synchronous and
// asynchronous.  A domain-event handler is asynchronous if it returns a Task.
// *****************************************************************************************
public void RunPublishDomainEventAsyncExample()
{
	nameof(RunPublishDomainEventAsyncExample).Dump();

	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new TestDomainEvent3(5);
	
	domainEventSrv.PublishAsync(evt).Wait();

}

// -------------------------------------------------------------------------------------
// Example of a domain-event.
// -------------------------------------------------------------------------------------
public class TestDomainEvent3 : DomainEvent
{
	public TestDomainEvent3(int seconds)
	{
		this.Seconds = seconds;
	}

	public int Seconds {get;}
}

public class TestHandlerAsync1 : IMessageConsumer
{
	public async Task OnEvent(TestDomainEvent3 evt)
	{
		await Task.Run(() => { Thread.Sleep(evt.Seconds * 1000); });
		
		nameof(TestHandlerAsync1).Dump();
		evt.Dump();
	}
}

public class TestHandlerAsync2 : IMessageConsumer
{
	public async Task OnEvent(TestDomainEvent3 evt)
	{
		await Task.Run(() => { Thread.Sleep(evt.Seconds * 1000); });

		nameof(TestHandlerAsync2).Dump();
		evt.Dump();
	}
}

// *********************************************************************************************
// A DomainCommand event is simular to a DomainEvent but it can have only one associated event
// handler.  If more than one event handler is found, an exception is thrown when AppContainer
// be built.  A DomainCommand can also have an optional return value.  Where a DomainEvent is
// published to notify consumers of something happening, a DomainCommand is used to notify a
// consumer that a certain action is to be taken.  Simular to DomainEvent handlers, a Domain-
// Command handler can be synchronous or asynchronous
// *********************************************************************************************
public void RunPublishCommandEventAsyncExample()
{
	nameof(RunPublishCommandEventAsyncExample).Dump();

	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var cmd = new TestDomainCommand(3, 5);

	domainEventSrv.PublishAsync(cmd).Wait();
	cmd.Result.Dump();
}

public class TestCommandResult
{
	public int Sum { get; }

	public TestCommandResult(int sum)
	{
		this.Sum = sum;
	}
}

public class TestDomainCommand : Command<TestCommandResult>
{
	public int Value1 { get; }
	public int Value2 { get; }
	
	public TestDomainCommand(int value1, int value2)
	{
		this.Value1 = value1;
		this.Value2 = value2;
	}
}

public class TestCommandHandler : IMessageConsumer
{
	public async Task OnEvent(TestDomainCommand cmd)
	{
		await Task.Run(() => { Thread.Sleep(cmd.Value1 * 1000); });
		
		cmd.Result = new TestCommandResult(cmd.Value1 + cmd.Value2);

		nameof(TestCommandHandler).Dump();
		cmd.Dump();
	}
}

// *********************************************************************************************
// The following shows how a dispatch rule can be assigned to an event handler to determine
// if it should handle the event.  Dispatch rules should be simple and based on the state of
// the domain-event.  For this reason, the dispatch rules are not registered within Autofac.
// *********************************************************************************************
public void RunDispatchRuleExample()
{
	nameof(RunDispatchRuleExample).Dump();

	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new TestDomainEvent4(3000, 8);

	domainEventSrv.PublishAsync(evt).Wait();
}

public abstract class VersionedEvent : DomainEvent
{
	public int Version { get; }

	public VersionedEvent(int version)
	{
		this.Version = version;
	}
}

public class TestDomainEvent4 : VersionedEvent
{
	public int Value { get; }

	public TestDomainEvent4(int value, int version) : base(version)
	{
		this.Value = value;
	}
}

public class TestDomainEventDispatchRuleHandler : IMessageConsumer
{
	[ApplyDispatchRule(typeof(NewVersionDispatchRule))]
	public void OnEventVer1(TestDomainEvent4 evt)
	{
		nameof(OnEventVer1).Dump();
		evt.Dump();
	}

	[ApplyDispatchRule(typeof(NewVersionDispatchRule), typeof(OverMaxDispatchRule))]
	public void OnEventVer2(TestDomainEvent4 evt)
	{
		nameof(OnEventVer2).Dump();
		evt.Dump();
	}
}

public class NewVersionDispatchRule : MessageDispatchRule<VersionedEvent>
{
	protected override bool IsMatch(VersionedEvent evt)
	{
		return evt.Version > 5;
	}
}

public class OverMaxDispatchRule : MessageDispatchRule<TestDomainEvent4>
{
	protected override bool IsMatch(TestDomainEvent4 evt)
	{
		return evt.Value >= 1000;
	}
}

// *********************************************************************************************
// An event handler can be declared to be a base event type.  If the event parameter has the
// IncludeDerivedEvents attribute specified, the handler will also be invoked for all types
// deriving from the parameter type.
// *********************************************************************************************
public void RunBaseEventHandlerExample()
{
	nameof(RunDispatchRuleExample).Dump();

	var domainEventSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new TestDomainEvent5(20, 8);

	domainEventSrv.PublishAsync(evt).Wait();
}

public class TestDomainEvent5 : VersionedEvent
{
	public int Value { get; }

	public TestDomainEvent5(int value, int version) : base(version)
	{
		this.Value = value;
	}
}

public class TestDomainEventBaseHandler : IMessageConsumer
{
	public void OnEvent([IncludeDerivedMessages]VersionedEvent evt)
	{
		nameof(TestDomainEventBaseHandler).Dump();
		evt.Dump();
	}
}