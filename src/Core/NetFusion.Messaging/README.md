# Messaging Overview
The NetFusion.Messaging plug-in provides an technology independent in-memory implementation for publishing and subscribing to messages.  This plug-in provides a simplistic implementation that can be extended to use more advanced messaging patterns.  The goal is to use the simplest implementation for the current need and be able to switch to a more advanced implementation if needed.  Also, different messaging implementations can be used simultaneously allowing the developer to choose implementation that best suits the need.

# Setup

The following are the steps for declaring, publishing, and subscribing to messages.  First, the NetFusion.RabbitMQ Nuget package needs to be installed:

![image](../../../img/Nuget-NetFusion.RabbitMQ.png)

Once the Nuget package is installed, the plug-in needs to be configured.  The ___InProcessMessageProcessor___ needs to be active within the application container.  This is accomplished by adding the ___MessagingConfig___ to the container.  By default, the ___MessagingConfig___ automatically registers this publisher.  If the in-memory implementation of message publishing isn't needed, it can be removed by calling the ___ClearMessagePublishers___ method before adding other publisher implementations.  The following is an example configuration:

``` sharp
	
	AppContainer.Create(new[] { "Samples.*.dll" })
    	.WithConfigSection("netFusion", "mongoAppSettings")
         
        // Eventing Plug-in Configuration.
        .WithConfig((MessagingConfig config) =>
        {
        	config.AddMessagePublisherType<RabbitMqMessagePublisher>();
        })

        // Configure Settings Plug-in.  This tells the plug-in where to look for
        // injected application settings.
        .WithConfig((NetFusionConfig config) => {

        config.AddSettingsInitializer(
        	typeof(FileSettingsInitializer<>),
            typeof(MongoSettingsInitializer<>));
        })

        .Build()
        .Start();
``` 

The above configuration also shows the RabbitMQ message publisher being configured.  This allows the application host and its associated components to publish local in-memory and centralized queue based messages.

## Message Types
There are two types types of messages that can be published: Commands and Domain-Events.  Both types of messages derive from the base ___IMessage___ interface.  Plublishers provide implementations by implementing the ___ICommand___ and ___IDomainEvent___ interfaces.  The ___Command___ and ___DomainEvent___ base classes provide base implementations from which application specific commands and domain-events can derive.

* ___Command___
:  A command is a message indicating that an action is being requested that needs to be handled.  A command can have one and only one subscriber event-handler.  If more than one event-handler is found, the messaging plug-in will raise an exception during the bootstrap process.  Commands can also have an optional response that can be set by the subscriber's event-handler.  The following is an example of a command declaration:
  
``` csharp

	public class ChangeEmailStatus : Command<StatusResult>
    {
        public string CustomerId { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
    }
```


* ___Domain-Event___
:  A domain-event is a message that notifies other application components that a completed action has taken place.  This allows one or more subscriber event-handlers to react to the event.  Unlike a command, a domain-event can't have an associated response type.   The following is an example of a domain-event declaration:




* ___Domain-Event Source___
:  A domain-event source is not a type of message but any domain-entity implementing the IEventSource interface.  This interface defines an enumeration of IDomainEvents.  When a domain-entity implementing the IEventSource interface is published, the list of associated domain-events are published to subscribers.  The following is an example of an event source:


## Publisher
The publisher declares the event classes and their associated properties.  When possible, it is best to make commands and event classes immutable.  The ___IMessageService___ is used to publish messages to enlisted subscribers.  Following is an example of publishing an event:




Note that the publish methods are all asynchronous.  The event-handler methods declared by consumers can be either synchronous or asynchronous.  For domain-events that have multiple event-handlers, the handlers can be a combination of both synchronous and asynchronous methods.  



  

## Consumer




# Implementation Details
