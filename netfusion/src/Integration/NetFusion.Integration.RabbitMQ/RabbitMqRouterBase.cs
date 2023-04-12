using NetFusion.Integration.Bus;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Exchanges;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Queues;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Rpc;
using NetFusion.Integration.RabbitMQ.Rpc.Metadata;
using NetFusion.Messaging.Types;

namespace NetFusion.Integration.RabbitMQ;

public abstract class RabbitMqRouterBase : BusRouterBase
{
    protected RabbitMqRouterBase(string busName) : base(busName)
    {
    }
    
    // -- Work Queue Message Patterns (Defining Microservice) ---

    /// <summary>
    /// Defines a work queue for a microservice to which other microservices send
    /// commands for processing.
    /// </summary>
    /// <param name="route">Contains data about how the queue should be created and
    /// how commands, sent to the queue, are routed to a consumer.</param>
    /// <typeparam name="TCommand">The type of command received on the queue.</typeparam>
    protected void DefineQueue<TCommand>(
        Action<CommandRouteWithMeta<TCommand, QueueMeta<TCommand>>> route)
        where TCommand : ICommand
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var command = new CommandRouteWithMeta<TCommand, QueueMeta<TCommand>>();
        ApplyDefaultQueueProperties(command.RouteMeta);
        
        route(command);

        string? queueName = command.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new RabbitMqPluginException("Queue name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(command);
        AddBusEntity(new QueueEntity(BusName, queueName, dispatcher));
    }

    /// <summary>
    /// Defines a work queue for a microservice to which other microservices send
    /// commands for processing.  The consumer will return a response that will
    /// optionally be sent to the microservice who sent the original command.
    /// </summary>
    /// <param name="route">Contains data about how the queue should be created and
    /// how commands, sent to the queue, are routed to a consumer.</param>
    /// <typeparam name="TCommand">The type of command received on the queue.</typeparam>
    /// <typeparam name="TResponse">The type of the response returned.</typeparam>
    protected void DefineQueueWithResponse<TCommand, TResponse>(
        Action<CommandRouteWithMeta<TCommand, TResponse, QueueMeta<TCommand>>> route)
        where TCommand : ICommand<TResponse>
        where TResponse : IMessage
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var command = new CommandRouteWithMeta<TCommand, TResponse, QueueMeta<TCommand>>();
        ApplyDefaultQueueProperties(command.RouteMeta);
        
        route(command);

        string? queueName = command.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new RabbitMqPluginException("Queue name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(command);
        AddBusEntity(new QueueEntity(BusName, queueName, dispatcher));
    }
    
    // -- Work Queue Message Patterns (Consuming Microservice) ---
    
    /// <summary>
    /// Routes a command to a queue for processing.
    /// </summary>
    /// <param name="queueName">The name of the queue to which the command should be sent.</param>
    /// <param name="options">The publish options used when the command is sent.</param>
    /// <typeparam name="TCommand">The type of the command sent to the queue.</typeparam>
    protected void RouteToQueue<TCommand>(string queueName, Action<PublishOptions>? options = null)
        where TCommand : ICommand
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
        
        var queueReference = new QueueReferenceEntity(typeof(TCommand), BusName, queueName);
        options?.Invoke(queueReference.PublishOptions);
        AddBusEntity(queueReference);
    }
    
    /// <summary>
    /// Routes a command to a queue for processing for which a response is returned on a reply queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to which the command should be sent.</param>
    /// <param name="route">Contains data about how the reply queue should be created.</param>
    /// <param name="options">The publish options used when the command is sent.</param>
    /// <typeparam name="TCommand">The type of the command sent to the queue.</typeparam>
    /// <typeparam name="TResponse">The type of the response sent on the reply queue.</typeparam>
    protected void RouteToQueueWithResponse<TCommand, TResponse>(string queueName,
        Action<CommandRouteWithMeta<TResponse, QueueMeta<TResponse>>> route,
        Action<PublishOptions>? options = null)
        where TCommand : ICommand<TResponse>
        where TResponse : ICommand
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
        
        // Defines queue to which command should be published:
        var queueReference = new QueueReferenceEntity(typeof(TCommand), BusName, queueName);
        options?.Invoke(queueReference.PublishOptions);
        AddBusEntity(queueReference);
        
        // Defines the queue on which the response to the command should be received:
        var command = new CommandRouteWithMeta<TResponse, QueueMeta<TResponse>>();
        ApplyDefaultQueueProperties(command.RouteMeta);
        
        route(command);
        
        string? replyQueueName = command.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(replyQueueName))
        {
            throw new RabbitMqPluginException("Queue name must be specified.");
        }

        // Link the two queues:
        queueReference.ReplyQueueName = replyQueueName;
        
        var dispatcher = new MessageDispatcher(command);
        AddBusEntity(new QueueEntity(BusName, replyQueueName, dispatcher));
    }
    
    // -- Exchange Message Patterns (Defining Microservice) ---
    
    /// <summary>
    /// Defines an exchange for a microservice to which it can publish domain-events to notify
    /// one or more other microservices of a change.
    /// </summary>
    /// <param name="meta">Contains data about how the exchange should be created.</param>
    /// <typeparam name="TDomainEvent">The type of the domain-event associated with the exchange.</typeparam>
    protected void DefineExchange<TDomainEvent>(Action<ExchangeMeta<TDomainEvent>> meta)
        where TDomainEvent : IDomainEvent
    {
        if (meta == null) throw new ArgumentNullException(nameof(meta));
        
        var exchangeMeta = new ExchangeMeta<TDomainEvent>
        {
            IsAutoDelete = false,
            IsDurable = true,
        };
        
        meta.Invoke(exchangeMeta);

        if (string.IsNullOrWhiteSpace(exchangeMeta.ExchangeName))
        {
            throw new RabbitMqPluginException("Exchange name must be specified.");
        }

        var exchangeEntity = new ExchangeEntity(
            typeof(TDomainEvent), 
            BusName, 
            exchangeMeta.ExchangeName,
            exchangeMeta);
        
        AddBusEntity(exchangeEntity);
    }
    
    // -- Exchange Message Patterns (Consuming Microservice) ---
    
    /// <summary>
    /// Defines a queue on an exchange to which another microservice publishes domain-events
    /// based on a route-key.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to bind the queue.</param>
    /// <param name="routeKeys">Specifies the keys for which one must match the key of the
    /// domain-event, published to the exchange, for it to be delivered to the queue.</param>
    /// <param name="route">Contains data about how the queue should be created.</param>
    /// <typeparam name="TDomainEntity">The type of the domain-entity published to the
    /// exchange and delivered to the queue.</typeparam>
    protected void SubscribeToExchange<TDomainEntity>(string exchangeName, string[] routeKeys, 
        Action<DomainRouteWithMeta<TDomainEntity, QueueMeta<TDomainEntity>>> route)
        where TDomainEntity : IDomainEvent
    {
        if (string.IsNullOrWhiteSpace(exchangeName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(exchangeName));

        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var domainEvent = new DomainRouteWithMeta<TDomainEntity, QueueMeta<TDomainEntity>>();
        ApplyDefaultQueueProperties(domainEvent.RouteMeta);

        route(domainEvent);
        
        string? queueName = domainEvent.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new RabbitMqPluginException("Queue name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(domainEvent);
        AddBusEntity(new SubscriptionEntity(
            BusName,  
            queueName, 
            exchangeName,
            routeKeys, 
            dispatcher));
    }
    
    /// <summary>
    /// Defines a queue on an exchange to which another microservices publishes domain-events.
    /// The domain-event is delivered to all queues bound to the exchange is not based on a
    /// matching route-key.
    /// </summary>
    /// <param name="exchangeName">The name of the exchange to bind the queue.</param>
    /// <param name="route">Contains data about how the queue should be created.</param>
    /// <param name="isPerServiceInstance">Indicates when multiple instances of a microservice
    /// are running in a cluster, each microservice instance should have its own queue.</param>
    /// <typeparam name="TDomainEntity">The type of the domain-event published to the
    /// exchange and delivered to the queue.</typeparam>
    protected void SubscribeToFanOutExchange<TDomainEntity>(string exchangeName,
        Action<DomainRouteWithMeta<TDomainEntity, QueueMeta<TDomainEntity>>> route,
        bool isPerServiceInstance = false)
        where TDomainEntity : IDomainEvent
    {
        if (string.IsNullOrWhiteSpace(exchangeName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(exchangeName));

        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var domainEvent = new DomainRouteWithMeta<TDomainEntity, QueueMeta<TDomainEntity>>();
        ApplyDefaultQueueProperties(domainEvent.RouteMeta);
        
        if (isPerServiceInstance)
        {
            domainEvent.RouteMeta.IsAutoDelete = true;
            domainEvent.RouteMeta.IsDurable = false;
            domainEvent.RouteMeta.IsExclusive = true;
        }
        
        route(domainEvent);
        
        string? queueName = domainEvent.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new RabbitMqPluginException("Queue name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(domainEvent);
        AddBusEntity(new SubscriptionEntity(
            BusName,  
            queueName, 
            exchangeName, 
            isPerServiceInstance,
            dispatcher));
    }
    
    // -- RPC Message Patterns (Defining Microservice) ---
    
    /// <summary>
    /// Allows a microservice to define a queue on which it receives RPC commands.  Each received
    /// RPC command is tagged with a message-namespace used to determine how the command is routed
    /// to its consumer.
    /// </summary>
    /// <param name="queueName">The name of the queue to create.</param>
    /// <param name="queueMeta">Details about the queue to create.</param>
    /// <exception cref="ArgumentException"></exception>
    protected void DefineRpcQueue(string queueName, Action<RpcQueueMeta>? queueMeta = null)
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
        
        var rpcQueueMeta = new RpcQueueMeta(queueName);
        queueMeta?.Invoke(rpcQueueMeta);

        var rpcEntity = new RpcEntity(BusName, rpcQueueMeta.QueueName, rpcQueueMeta);
        AddBusEntity(rpcEntity);
    }

    /// <summary>
    /// Specifies the consumer to which the received RPC message should be routed.
    /// </summary>
    /// <param name="queueName">The name of the RPC queue on which the RPC Command is received.</param>
    /// <param name="route">The route specifying the consumer and handler of the RPC Command.</param>
    /// <param name="messageNamespace">The namespace of the RPC command associated with the route.
    /// If not specified, the namespace specified as a message attribute will be used.</param>
    /// <typeparam name="TCommand">The type of command to be routed.</typeparam>
    /// <typeparam name="TResult">The type of the response returned from the command handler.</typeparam>
    protected void DefineRpcQueueRoute<TCommand, TResult>(
        string queueName,
        Action<CommandRoute<TCommand, TResult>> route,
        string? messageNamespace = null)
        where TCommand : ICommand<TResult>
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
        
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        // Multiple commands identified by namespace are sent on same queue:
        RpcEntity rpcEntity = ResolveRpcQueue(queueName);
        string rpcMessageNamespace = ResolveMessageNamespace(typeof(TCommand), messageNamespace);
        
        var command = new CommandRoute<TCommand, TResult>();
        route(command);
        
        rpcEntity.AddMessageDispatcher(rpcMessageNamespace, new MessageDispatcher(command));
    }
    
    // -- RPC Message Patterns (Consuming Microservice) ---
    
    /// <summary>
    /// Called to specify the queue to which a RPC command should be published.
    /// </summary>
    /// <param name="queueName">The name of the queue to which the RPC command should be published.</param>
    /// <param name="messageNamespace">The optional message namespace for the RPC command.  If not specified,
    /// the message namespace must be specified on the command's type using the RpcMessageNamespace attribute.</param>
    /// <param name="publishOptions">The optional publish options to use.</param>
    /// <typeparam name="TCommand">The type of the command to be published.</typeparam>
    protected void RouteToRpcQueue<TCommand>(string queueName,
        string? messageNamespace = null,
        Action<RpcPublishOptions>? publishOptions = null)
        where TCommand : ICommand
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

        var rpcEntity = BusEntities.OfType<RpcReferenceEntity>().FirstOrDefault(re => re.EntityName == queueName);
        if (rpcEntity == null)
        {
            rpcEntity = new RpcReferenceEntity(BusName, queueName);
            AddBusEntity(rpcEntity);
        }

        publishOptions?.Invoke(rpcEntity.PublishOptions);
        
        var commandType = typeof(TCommand);
        var rpcMessageNamespace = ResolveMessageNamespace(commandType, messageNamespace);
        
        rpcEntity.AddCommandMessageNamespace(commandType, rpcMessageNamespace);
    }
    
    private static void ApplyDefaultQueueProperties(QueueMeta queueRouteMeta)
    {
        queueRouteMeta.IsAutoDelete = false;
        queueRouteMeta.IsDurable = true;
        queueRouteMeta.IsExclusive = false;
    }
    
    private RpcEntity ResolveRpcQueue(string queueName) => BusEntities.OfType<RpcEntity>()
        .FirstOrDefault(ne => ne.EntityName == queueName) ?? throw new RabbitMqPluginException(
            $"An existing RPC Queue named {queueName} not found within namespace {BusName}.", 
            "RPC_QUEUE_NOTFOUND");
    
    private static string ResolveMessageNamespace(Type messageType, string? messageNamespace) =>
        messageNamespace ?? messageType.GetAttribute<MessageNamespaceAttribute>()?.MessageNamespace ??
        throw new RabbitMqPluginException(
            $"RPC message namespace could not be found for command {messageType}. The namespace must be specified " +
            $"on the command using the {nameof(MessageNamespaceAttribute)} or when mapping the command to the queue.", 
            "RPC_MSG_NS_NOTFOUND");
}