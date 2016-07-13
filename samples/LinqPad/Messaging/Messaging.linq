<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Messaging.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Messaging.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Newtonsoft.Json.dll</Reference>
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

// These examples show using the Messaging Plug-in for publishing local domain
// events and commands.By default, the InProcessEventDispatcher is configured which
// implements the dispatching of messages to handlers defined within the local process.
// The messaging plug -in defines an extension point where other publisher implementations
// can be called to handle the dispatching of messages(i.e.RabbitMQ).
// 
// A message can be published to one or more event handlers.Message handlers can be
// synchronous or asynchronous.The message handler definition determines if the handler
// is invoked synchronously or asynchronously and not the domain -event or command.
// The domain-event or command is just a simple POCO.  Since message handlers can be 
// either synchronous or asynchronous, the method to publish messages is asynchronous.

// *****************************************************************************************
void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new TestTypeResolver(pluginDirectory,
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
	var messageSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
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
	var messageSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
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
	var messageSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new ExampleAsyncDomainEventException(info);

	try
	{
		messageSrv.PublishAsync(evt).Wait();
	}
	catch (PublisherException ex)
	{
		return ex.PublishDetails.ToIndentedJson();
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
	var messageSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
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
	var messageSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new ExampleRuleDomainEvent(info);
	messageSrv.PublishAsync(evt).Wait();
	return evt.Attributes;
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
			this.Attributes["__low__"] = "";
		}
	}
}

public class IsLowImportance : MessageDispatchRule<DomainEvent>
{
	protected override bool IsMatch(DomainEvent message)
	{
		return message.Attributes.Keys.Contains("__low__");
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
		evt.Attributes["IsLowImportance"] = "Event is of low importance.";
	}

	[InProcessHandler, ApplyDispatchRule(typeof(IsHighImportance))]
	public void OnEvent(ExampleRuleDomainEvent evt)
	{
		evt.Attributes["IsHighImportance"] = "Event is of high importance.";
	}
}

// --------------------------------------------------------------------------
// This example shows how to declare a message handler that will be called
// for any message types derived from the handler's declared parameter type.
// The parameter is marked with the IncludeDerivedMessages attribute.
// --------------------------------------------------------------------------
public IDictionary<string, object> RunSyncDerivedEvent()
{
	var messageSrv = AppContainer.Instance.Services.Resolve<IMessagingService>();
	var evt = new ExampleDerivedDomainEvent();
	messageSrv.PublishAsync(evt).Wait();
	return evt.Attributes;
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
		evt.Attributes["Message1"] = "Base Handler Called";
	}

	[InProcessHandler]
	public void OnEvent(ExampleDerivedDomainEvent evt)
	{
		evt.Attributes["Message"] = "Derived Handler Called";
	}
}