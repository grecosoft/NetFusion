<Query Kind="Program">
  <NuGetReference>NetFusion.Base</NuGetReference>
  <NuGetReference>NetFusion.Bootstrap</NuGetReference>
  <NuGetReference>NetFusion.Common</NuGetReference>
  <NuGetReference>NetFusion.Domain.Messaging</NuGetReference>
  <NuGetReference>NetFusion.Messaging</NuGetReference>
  <NuGetReference>NetFusion.Test</NuGetReference>
  <Namespace>Autofac</Namespace>
  <Namespace>NetFusion.Bootstrap.Configuration</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Exceptions</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Domain.Messaging</Namespace>
  <Namespace>NetFusion.Domain.Messaging.Rules</Namespace>
  <Namespace>NetFusion.Domain.Modules</Namespace>
  <Namespace>NetFusion.Messaging</Namespace>
  <Namespace>NetFusion.Test.Container</Namespace>
  <Namespace>NetFusion.Test.Plugins</Namespace>
  <Namespace>NetFusion.Testing.Logging</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>NetFusion.Messaging.Rules</Namespace>
</Query>

private AppContainer Container { get; set; }

void Main()
{
	var resolver = new TestTypeResolver(this.GetType());
	var hostPlugin = new MockAppHostPlugin();
	var corePlugin = new MockCorePlugin();
	
	corePlugin.UseMessagingPlugin();
	corePlugin.AddPluginType<DomainScriptingModule>();

	resolver.AddPlugin(hostPlugin, corePlugin);
	this.Container = ContainerSetup.Bootstrap(resolver);

	// Build and start the container.
	Container.Build();
	Container.Start();

	// Execute the examples:
	RunDomainEventSync(new MessageInfo { DelayInSeconds = 5 }).Dump();
	RunDomainEventAsync(new MessageInfo { DelayInSeconds = 5 }).Dump();
	RunAsyncCommand(new CommandInfo { InputMessage = "test", Seconds = 1 }).Dump();
	RunEventWithRule(new MessageRuleInfo { Value = 500 }).Dump();
	RunSyncDerivedEvent().Dump();
	// RunAsyncEventException(new MessageExInfo {}).Dump();
}


// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{

}

// -------------------------------------------------------------------------------------
// This example shows publishing a domain event to two synchronous handlers.
// Each handler method puts the current thread to sleep for the specified
// number of seconds.There are two handlers for this event so the time
// to execute will be 2 x specified-seconds.
// -------------------------------------------------------------------------------------
public string RunDomainEventSync(MessageInfo info)
{
	var messageSrv = Container.Services.Resolve<IMessagingService>();
	Stopwatch stopwatch = new Stopwatch();

	stopwatch.Start();

	var evt = new ExampleSyncDomainEvent(info);
	messageSrv.PublishAsync(evt).Wait();

	stopwatch.Stop();
	return new { Elapsed = stopwatch.Elapsed }.ToIndentedJson();
}

public class MessageInfo : DomainEvent
{
	public int DelayInSeconds { get; set; }
}

public class ExampleSyncDomainEvent : DomainEvent
{
	public int Seconds { get; set; }

	public ExampleSyncDomainEvent(MessageInfo info)
	{
		this.Seconds = info.DelayInSeconds;
	}
}

public class ExampleSyncHandler1 : IMessageConsumer
{
	[InProcessHandler]
	public void OnEvent(ExampleSyncDomainEvent evt)
	{
		Thread.Sleep(evt.Seconds * 1000);
	}
}

public class ExampleSyncHandler2 : IMessageConsumer
{
	[InProcessHandler]
	public void OnEvent(ExampleSyncDomainEvent evt)
	{
		Thread.Sleep(evt.Seconds * 1000);
	}
}

// ------------------------------------------------------------------------------------------------
// This example shows publishing a domain event to two asynchronous handlers.Each handler
// creates a new task that puts the current thread to sleep for the specified number of seconds.  
// There are two handlers for this event that are asynchronously executed so the time should be
// ------------------------------------------------------------------------------------------------
public string RunDomainEventAsync(MessageInfo info)
{
	var messageSrv = Container.Services.Resolve<IMessagingService>();
	Stopwatch stopwatch = new Stopwatch();

	stopwatch.Start();

	var evt = new ExampleAsyncDomainEvent(info);
	messageSrv.PublishAsync(evt).Wait();

	stopwatch.Stop();
	return new { Elapsed = stopwatch.Elapsed }.ToIndentedJson();
}

public class ExampleAsyncDomainEvent : DomainEvent
{
	public int Seconds { get; set; }

	public ExampleAsyncDomainEvent(MessageInfo info)
	{
		this.Seconds = info.DelayInSeconds;
	}
}

public class ExampleAsyncHandler1 : IMessageConsumer
{
	[InProcessHandler]
	public async Task OnEvent(ExampleAsyncDomainEvent evt)
	{
		await Task.Run(() => { Thread.Sleep(evt.Seconds * 1000); });
	}
}

public class ExampleAsyncHandler2 : IMessageConsumer
{
	[InProcessHandler]
	public async Task OnEvent(ExampleAsyncDomainEvent evt)
	{
		await Task.Run(() => { Thread.Sleep(evt.Seconds * 1000); });
	}
}

// -------------------------------------------------------------------------------------
// This example publishes an event that is handled by two asynchronous handlers
// that each throw an exception.The details of the PublisherException are
// returned to the calling client.
// -------------------------------------------------------------------------------------
public string RunAsyncEventException(MessageExInfo info)
{
	var messageSrv = Container.Services.Resolve<IMessagingService>();
	var evt = new ExampleAsyncDomainEventException(info);

	try
	{
		messageSrv.PublishAsync(evt).Wait();
	}
	catch (PublisherException ex)
	{
		return ex.Data.ToIndentedJson();
	}

	return "Expected Exception not Raised.";
}

public class MessageExInfo : DomainEvent
{
	public int DelayInSeconds = 5;
	public bool ThrowEx = true;
}

public class ExampleAsyncDomainEventException : DomainEvent
{
	public int Seconds { get; set; }
	public bool ThrowEx { get; set; }

	public ExampleAsyncDomainEventException(MessageExInfo info)
	{
		this.Seconds = info.DelayInSeconds;
		this.ThrowEx = info.ThrowEx;
	}
}

public class ExampleAsyncHandler4 : IMessageConsumer
{
	[InProcessHandler]
	public async Task OnEvent(ExampleAsyncDomainEventException evt)
	{
		await Task.Run(() =>
		{
			Thread.Sleep(evt.Seconds * 1000);
		});

		if (evt.ThrowEx)
		{
			throw new InvalidOperationException($"Example exception: {Guid.NewGuid()}");
		}
	}
}

// ----------------------------------------------------------------------------------------------
// This example publishes a command with a typed result.A command message type can have one and 
// only one handler.If more than one handler is found, an exception will be thrown during the 
// bootstrap process.
// ----------------------------------------------------------------------------------------------
public HandlerResponse RunAsyncCommand(CommandInfo info)
{
	var messageSrv = Container.Services.Resolve<IMessagingService>();
	var cmd = new ExampleAsyncCommand(info);
	return messageSrv.PublishAsync(cmd).Result;
}

public class CommandInfo
{
	public string InputMessage { get; set; }
	public int Seconds { get; set; }
}

public class HandlerResponse
{
	public string ResponseMessage { get; set; }
}

public class ExampleAsyncCommand : Command<HandlerResponse>
{
	public string Message { get; set; }
	public int Seconds { get; set; }

	public ExampleAsyncCommand(CommandInfo info)
	{
		this.Message = info.InputMessage;
		this.Seconds = info.Seconds;
	}
}

public class ExampleAsyncHandler5 : IMessageConsumer
{
	[InProcessHandler]
	public async Task<HandlerResponse> OnCommand(ExampleAsyncCommand command)
	{
		await Task.Run(() =>
		{
			Thread.Sleep(command.Seconds * 1000);
		});

		return new HandlerResponse
		{
			ResponseMessage = command.Message + " - with handler response. "
		};
	}
}

// ------------------------------------------------------------------------------------------
// This example shows have derived MessageDispatchRule types can be added to a message
// handler to determine if it should be invoked based on the state of the message.
// ------------------------------------------------------------------------------------------
public IDictionary<string, object> RunEventWithRule(MessageRuleInfo info)
{
	var messageSrv = Container.Services.Resolve<IMessagingService>();
	var evt = new ExampleRuleDomainEvent(info);
	messageSrv.PublishAsync(evt).Wait();
	return evt.AttributeValues;
}

public class MessageRuleInfo
{
	public int Value { get; set; }
}

public class ExampleRuleDomainEvent : DomainEvent
{
	public int Value { get; }

	public ExampleRuleDomainEvent(MessageRuleInfo info)
	{
		this.Value = info.Value;

		if (this.Value == 50)
		{
			this.Attributes.SetValue("__low__", "");
		}
	}
}

public class IsLowImportance : MessageDispatchRule<DomainEvent>
{
	protected override bool IsMatch(DomainEvent message)
	{
		return message.Attributes.Contains("__low__");
	}
}

public class IsHighImportance : MessageDispatchRule<ExampleRuleDomainEvent>
{
	protected override bool IsMatch(ExampleRuleDomainEvent message)
	{
		return message.Value > 100;
	}
}

public class ExampleRuleHandler : IMessageConsumer
{
	[InProcessHandler, ApplyDispatchRule(typeof(IsLowImportance))]
	public void OnEvent([IncludeDerivedMessages]DomainEvent evt)
	{
		evt.Attributes.SetValue("IsLowImportance", "Event is of low importance.");
	}

	[InProcessHandler, ApplyDispatchRule(typeof(IsHighImportance))]
	public void OnEvent(ExampleRuleDomainEvent evt)
	{
		evt.Attributes.SetValue("IsHighImportance", "Event is of high importance.");
	}
}

// --------------------------------------------------------------------------
// This example shows how to declare a message handler that will be called
// for any message types derived from the handler's declared parameter type.
// The parameter is marked with the IncludeDerivedMessages attribute.
// --------------------------------------------------------------------------
public IDictionary<string, object> RunSyncDerivedEvent()
{
	var messageSrv = Container.Services.Resolve<IMessagingService>();
	var evt = new ExampleDerivedDomainEvent();
	messageSrv.PublishAsync(evt).Wait();
	return evt.AttributeValues;
}

public class ExampleBaseDomainEvent : DomainEvent
{

}

public class ExampleDerivedDomainEvent : ExampleBaseDomainEvent
{

}

public class ExampleHierarchyHandler : IMessageConsumer
{
	[InProcessHandler]
	public void OnEvent([IncludeDerivedMessages]ExampleBaseDomainEvent evt)
	{
		evt.Attributes.SetValue("Message1", "Base Handler Called");
	}

	[InProcessHandler]
	public void OnEvent(ExampleDerivedDomainEvent evt)
	{
		evt.Attributes.SetValue("Message", "Derived Handler Called");
	}
}