# RabbitMQ Overview
>The RabbitMQ plug-in integrates with the core eventing plug-in and allows publishing and receiving domain events from queues.  Since this plugin integrates with the default eventing plugin, publishing and receiving events work the same as it does for local in-process domain events.

# Setup
Add the NetFusion.RabbitMQ NuGet package to the projects that will be defining exchanges and event handlers.  The following sections show how to define exchanges and queues that can be used for publishing and receiving events.  The following needs to be configured in both the publishing and subscribing host applications:

``` csharp
var netFusionConfig = (NetFusionSection)ConfigurationManager.GetSection("netFusion");

AppContainer.Create(AppDomain.CurrentDomain.RelativeSearchPath)
     .WithConfig(netFusionConfig)

     // Eventing Plugin Configuration.
     .WithConfig((DomainEventConfig config) =>
     {
         config.AddFilterType<RabbitMqEventFilter>();
     })

     // Configure Settings Plugin.  This tells the plugin where to look for
     // injected application settings.
     .WithConfig((AppConfig config) => {

         config.AddSettingsInitializer(
             typeof(FileSettingsInitializer<>),
             typeof(MongoSettingsInitializer<>));
     })

     .Build()
     .Start();
}
```

## Publishers
Publishers define exchanges and queues associated with a specific domain event type.  When an event is published using the ***IEventingService*** interface, a message containing the event will be placed on the associated queue.  Multiple exchanges can be defined for an event type.  The code to publish and handle queue based event messages is identical to the code for in-process event publishing and handling.

The following is an example of defining a direct exchange for the sample DirectEvent:

``` csharp
public class DirectExchange : DirectExchange<DirectEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";
        Settings.ExchangeName = "SampleDirectExchange";

        QueueDeclare("2015-2016-Cars", config => {
            config.RouteKeys = new[] { "2015", "2016" };
        });

        QueueDeclare("UsedCars", config => {
            config.RouteKeys = new[] { "UsedModel" };
        });
    }
}
```

When the exchange is created, the name of the configured broker is used.  In this case, the key to identify the RabbitMQ instance is:  TestBroker.  The host applications must configure ***BrockerSettings*** class as follows:

``` json
{
    "Connections" : [
        {
            "BrokerName" : "TestBroker",
            "HostName" : "LocalHost"
        }
    ]
}
```  

The code to publish a event is as follows:

``` csharp
public async Task PublishDirectEvent(CarSubmission car)
{
        var directEvt = new DirectEvent
    {
        Make = car.Make,
        Model = car.Model,
        Year = car.Year,
        CurrentDateTime = DateTime.Now,
        Vin = Guid.NewGuid().ToString()
    };

    directEvt.SetRouteKey(car.Year);

    if (car.Year < 2015)
    {
        directEvt.SetRouteKey("UsedModel");
    }

    await _domainEventService.PublishAsync(directEvt);
}
```

Additional details of the different types of exchanges will be discussed in later sections.  However, this plug-in provides a very light implementation on top of the RabbitMQ client driver so all documentation provided on the RabbitMQ website applies.

## Consumers
Consumers subscribe to queue based events using the same code as subscribing to in-process domain events.  The only additional configuration is the specification of an attribute determining how the consumer enlists with the exchange.  The following is the code used to subscribe to the event being published in the last example.

``` csharp
Broker("TestBroker")]
public class DirectExchangeService : IDomainEventConsumer
{
    [JoinQueue("UsedCars", "SampleDirectExchange")]
    public void OnUsedCars(DirectEvent directEvt)
    {
        Console.WriteLine(
            "Handler: OnUsedCars[UsedCars]: {0}".FormatUsing(
                directEvt.ToJson()));

        directEvt.SetAcknowledged();
    }

    [AddQueue("SampleDirectExchange", RouteKey = "UsedModel",
        IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
    public void OnOlderModelYear(DirectEvent directEvt)
    {
        Console.WriteLine(
            "Handler: OnOlderModelYear[UsedCars]: {0}".FormatUsing(
                directEvt.ToJson()));
    }
}
```

The class containing the event handlers must implement the ***IDomainEventConsumer*** marker interface and specify the associated broker using the ***Broker*** class attribute.  In addition, one of the following attributes are used to subscribe to event messages being delivered to a queue:

* ___JoinQueue___
:  This subscribes the event handler to the queue by joining all other subscribed consumers.  When an event is published, the message is delivered to all consumers round-robin.

* ___AddQueue___
:  This creates a new temporary unnamed queue on the exchange and subscribes the event handler.  In this case, the event handler will be invoked for every message published to the exchange.

# Implementation
The RabbitMQ plug-in is implemented by registering an implementation of the ***IDomainEventFilter*** with the ***DomainEventConfig*** when the application is bootstrapped.  This filter determines if there is an exchange configured for the type of event and will publish it to the exchange.  The filter delegates to the ***DomainEventBrokerModule*** to publish events.  When this module is initialized during the bootstrap process, it scans for all of the exchanges and handlers and stores the metadata used at runtime.  When the ***DomainEventBrokerModule*** is started, it will create the needed exchanges and queues.  It will also subscribe to all event handlers with the appropriate attributes.  This section will not go into details of the implementation, but the ***DomainEventBroker*** class contains the code that interfaces with the RabbitMQ client.

## Exchanges
The following will discuss each RabbitMQ exchange types and how they  are configured.  The design is based directly on the RabbitMQ client driver so all RabbitMQ documentation applies.  The notes on the functioning of each type of exchange/queue is based on this excellent RabbitMQ book:  

[___RabbitMQ in Action: Distributed Messaging for Everyone___](http://www.amazon.com/RabbitMQ-Action-Distributed-Messaging-Everyone/dp/1935182978/ref=sr_1_1?s=books&ie=UTF8&qid=1455555212&sr=1-1&keywords=rabbitmq)

### Direct Exchanges
The direct exchange uses the route-key to determine which queues should receive the message.  When a queue is bound to an exchange, it can specify a route key value.  When messages are published to a direct exchange, the publisher specifies a route-key value.  The exchange will deliver the message to all queues that have a binding with the specified route key-value.  A given queue can be bound more than once to an exchange - each binding using a different route-key.

__Publisher:__

``` csharp
public class DirectExchange : DirectExchange<DirectEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";
        Settings.ExchangeName = "SampleDirectExchange";

        QueueDeclare("2015-2016-Cars", config => {
            config.RouteKeys = new[] { "2015", "2016" };
        });

        QueueDeclare("UsedCars", config => {
            config.RouteKeys = new[] { "UsedModel" };
        });
    }
}
```

__Subscriber:__

``` csharp
[Broker("TestBroker")]
public class DirectExchangeService : IDomainEventConsumer
{
    // This method will join to the 2015-2016-Cars queue defined on the
    // ExampleDirectExchange.  Since this handler is joining the queue,
    // it will be called round-robin with with other subscribed clients.
    [JoinQueue("2015-2016-Cars", "SampleDirectExchange")]
    public void OnModelYear(DirectEvent directEvt)
    {
        Console.WriteLine($"Handler: OnModelYear[2015-2016-Cars]: {directEvt.ToJson()}");

        directEvt.SetAcknowledged();
    }

    // This method will join to the UsedCars queue defined on the
    // ExampleDirectExchange.  Since this handler is joining the queue,
    // it will be called round-robin with with other subscribed clients.
    [JoinQueue("UsedCars", "SampleDirectExchange")]
    public void OnUsedCars(DirectEvent directEvt)
    {
        Console.WriteLine(
            "Handler: OnUsedCars[UsedCars]: {0}".FormatUsing(
                directEvt.ToJson()));

        directEvt.SetAcknowledged();
    }

    // This event handler method will be associated with a new queue that
    // will be created on the exchange.  Since this will be a new queue
    // created on the exchange, it will not be called round-robin. (So
    // this and the OnUsedCars will be both called.  Also note that this
    // queue is configured to not require an acknowledgement.
    [AddQueue("SampleDirectExchange", RouteKey = "UsedModel",
        IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
    public void OnOlderModelYear(DirectEvent directEvt)
    {
        Console.WriteLine(
            "Handler: OnOlderModelYear[UsedCars]: {0}".FormatUsing(
                directEvt.ToJson()));
    }
}
```
### Topic Exchanges
The same as a direct exchange.  However, the route key value is a filter and not just a value.  The route-key used to specify a queue to exchange binding consists of a filter with '.' delimited values:  A.B.*

When a message is posted, the message will only be delivered to the queue if one of its binding filter values match the posted route-key value.

__Publisher:__

``` csharp
public class SampleTopicExchange : TopicExchange<TopicEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";
        Settings.ExchangeName = "SampleTopicExchange";

        QueueDeclare("Chevy", config => {
            config.RouteKeys = new[] { "Chevy.*.*" };
        });

        QueueDeclare("Chevy-Vette", config => {
            config.RouteKeys = new[] { "Chevy.Vette.*" };
        });

        QueueDeclare("Ford", config => {
            config.RouteKeys = new[] { "Ford.*.*", "Lincoln.*.*" };
        });
    }
}
```

__Subscriber:__

``` csharp
[Broker("TestBroker")]
public class TopicExchangeService : IDomainEventConsumer
{
    // This method will join the Chevy queue defined on the SampleTopicExchange
    // exchange.  Since it is joining an existing queue, it will join any other
    // enlisted subscribers and be called round-robin.  
    [JoinQueue("Chevy", "SampleTopicExchange")]
    public void OnChevy(TopicEvent topicEvt)
    {
        Console.WriteLine(
            "Handler: OnChevy: {0}".FormatUsing(
                topicEvt.ToJson()));

        topicEvt.SetAcknowledged();
    }

    // This event handler will join the Chevy-Vette queue defined on the
    // SampleTopicExchange.  This handler is like the prior one, but the
    // associated queue has a more specific route-key pattern.  Both this
    // handler and the prior one will both be called since this handler
    // has a more specific pattern to include the model of the car.
    [JoinQueue("Chevy-Vette", "SampleTopicExchange")]
    public void OnChevyVette(TopicEvent topicEvt)
    {
        Console.WriteLine(
            "Handler: OnChevyVette: {0}".FormatUsing(
                topicEvt.ToJson()));

        topicEvt.SetAcknowledged();
    }

    // This event handler joins the Ford queue defined on the same
    // exchange as the prior two event handlers.
    [JoinQueue("Ford", "SampleTopicExchange")]
    public void OnFord(TopicEvent topicEvt)
    {
        Console.WriteLine(
            "Handler: OnFord: {0}".FormatUsing(
                topicEvt.ToJson()));

        topicEvt.SetAcknowledged();
    }

    // This event handler creates a new queue on SampleTopicExchange
    // matching any route key.  However, this event handler has a
    // dispatch-role specified to only be called if the Make of the
    // car is <= three characters.  The event is still delivered but
    // just not passed to this handler.  If there are a large number
    // of events, it is best to create a dedicated queue on the
    // exchange.
    [ApplyDispatchRule(typeof(ShortMakeRule))]
    [AddQueue("SampleTopicExchange", RouteKey = "#",
        IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
    public void OnSortMakeName(TopicEvent topicEvt)
    {
        Console.WriteLine(
            "Handler: OnSortMakeName: {0}".FormatUsing(
                topicEvt.ToJson()));
    }

    // This adds a queue with a more specific pattern.  Since it
    // creating a new queue, it will be called in addition to the
    // event handler that is specified for the Ford queue.
    [AddQueue("SampleTopicExchange", RouteKey = "Ford.Mustang.*",
        IsAutoDelete = true, IsNoAck = true, IsExclusive = true)]
    public void OnFordMustang(TopicEvent topicEvt)
    {
        Console.WriteLine(
            "Handler: OnFordMustang: {0}".FormatUsing(
                topicEvt.ToJson()));
    }
}
```

### Fanout Exchanges

* An exchange of type Fanout will broadcast a message to all queues defined on the exchange.

* This type of configuration is often used when the message needs to be sent to many receivers.

* This configuration usually does not require the message to be acknowledged by the consumers.  This set-up is achieved by having each receiver define an queue that is bound to by the consumer and automatically deleted once the consumer disconnects.

* Route keys are not used by this type of exchange.

* Giving a queue a name is important when you want to share the queue between producers and consumers.  But in this case, the publisher usually does not know about the consumers.  In this case a temporary named queue is best.  Also, the consumer is usually not interested in old messages, just current ones.

* Firstly, whenever we connect to Rabbit we need a fresh, empty queue. To do this we could create a queue with a random name, or, even better - let the server choose a random queue name for us.  Secondly, once we disconnect the consumer the queue should be automatically deleted.

* The messages will be lost if no queue is bound to the exchange yet.  For most publish/subscribe scenarios this is what we would want.

__Publisher:__

``` csharp
public class GermanCarExchange : FanoutExchange<FanoutEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";
        Settings.ExchangeName = "SampleFanoutExchange->GermanCars";
    }

    protected override bool Matches(FanoutEvent domainEvent)
    {
        return domainEvent.Make.InSet("VW", "Audi", "BMW");
    }

}

public class AmericanCarExchange : FanoutExchange<FanoutEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";
        Settings.ExchangeName = "SampleFanoutExchange->AmericanCars";
    }

    protected override bool Matches(FanoutEvent domainEvent)
    {
        return domainEvent.Make.InSet("Ford", "GMC", "Chevy");
    }
}
```

__Subscriber:__

``` csharp
[Broker("TestBroker")]
public class FanoutExchangeService : IDomainEventConsumer
{
    [AddFanoutQueue("SampleFanoutExchange->GermanCars")]
    public void OnGermanCars(FanoutEvent fanoutEvt)
    {
        Console.WriteLine(
            "Handler: OnGermanCars: {0}".FormatUsing(
                fanoutEvt.ToJson()));
    }

    [AddFanoutQueue("SampleFanoutExchange->AmericanCars")]
    public void OnAmericanCars(FanoutEvent fanoutEvt)
    {
        Console.WriteLine(
            "Handler: OnAmericanCars: {0}".FormatUsing(
                fanoutEvt.ToJson()));
    }
}
```

### WorkQueue Exchanges
Used to distribute tasks published to the queue to multiple consumers in a round-robin fashion.

* Publisher message RouteKey == Queue Name.

* When configuring a workflow queue, it is defined using the default exchange.

* Consumers bind to a workflow queue by using the name assigned to the queue.

* Publishers publish messages by specifying the name of queue as the Route-Key.

* The message will be delivered to the queue having the same name as the route-key and delivered to bound consumers in a round robin sequence.

* This type of queue is used to distribute time intensive tasks to multiple consumers bound to the queue.

* The tasks may take several seconds to complete.  When the consumer is processing the task and fails, another bound consumer should be given the task.  This is achieved by having the client acknowledge the task once its processing has completed.

* There aren't any message timeouts; RabbitMQ will redeliver the message only when the worker connection dies. It's fine even if processing a message takes a very, very long time.

* For this type of queue, it is usually desirable to not loose the task messages if the RabbitMQ server is restarted or would crash.  Two things are required to make sure that messages aren't lost: we need to mark both the queue and messages as durable.

* If fair dispatch is enabled, RabbitMQ will not dispatch a message to a consumer if there is a pending acknowledgment.  This keeps a busy consumer from getting a backlog of messages to process.

* If all the workers are busy, your queue can fill up. You will want to keep an eye on that, and maybe add more workers, or have some other strategy.

__Publisher:__

``` csharp
public class SampleWorkQueueExchange : WorkQueueExchange<WorkQueueEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";

        QueueDeclare("ProcessSale", config =>
        {

        });

        QueueDeclare("ProcessService", config =>
        {

        });
    }
}
```

__Subscriber:__

``` csharp
[Broker("TestBroker")]
public class WorkQueueExchangeService : IDomainEventConsumer
{
    [JoinQueue("ProcessSale")]
    public void OnProcessSale(WorkQueueEvent workQueueEvent)
    {
        Console.WriteLine(
            "Handler: OnProcessSale: {0}".FormatUsing(
                workQueueEvent.ToJson()));

        workQueueEvent.SetAcknowledged();
    }

    [JoinQueue("ProcessService")]
    public void OnProcessService(WorkQueueEvent workQueueEvent)
    {
        Console.WriteLine(
            "Handler: OnProcessService: {0}".FormatUsing(
                workQueueEvent.ToJson()));

        workQueueEvent.SetAcknowledged();
    }

}
```

### RPC Exchanges
__Publisher:__

``` csharp
public class SampleRpcExchange : DirectExchange<RpcEvent>
{
    protected override void OnDeclareExchange()
    {
        Settings.BrokerName = "TestBroker";
        Settings.ExchangeName = "SampleRpcExchange";
        SetReturnType<RpcEvent>();

        QueueDeclare("QueueWithConsumerResponse", config => {
            config.RouteKeys = new[] { "Hello" };
        });
    }
}
```

__Subscriber:__

``` csharp
[Broker("TestBroker")]
public class RpcExchangeService : IDomainEventConsumer
{
    [JoinQueue("QueueWithConsumerResponse", "SampleRpcExchange")]
    public async Task<RpcEvent> OnRpcMessage(RpcEvent rpcEvent)
    {
        Console.WriteLine(
            "Handler: OnRpcMessage: {0}".FormatUsing(
                rpcEvent.ToJson()));

        rpcEvent.SetAcknowledged();

        await Task.Run(() => {
            Thread.Sleep(500);
        });

        return new RpcEvent
        {
            InputValue = "World"
        };
    }
}
```

#Resources

* [RabbitMQ]([http://www.rabbitmq.com/](http://www.rabbitmq.com/)
* [RabbitMQ Tutorial](http://www.rabbitmq.com/getstarted.html)
* [RabbitMQ In Action](http://www.amazon.com/RabbitMQ-Action-Distributed-Messaging-Everyone/dp/1935182978/ref=sr_1_sc_1?s=books&ie=UTF8&qid=1455671940&sr=1-1-spell&keywords=Rabbitmw)
