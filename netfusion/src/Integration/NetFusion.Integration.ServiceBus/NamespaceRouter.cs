using NetFusion.Integration.Bus;
using NetFusion.Integration.ServiceBus.Internal;
using NetFusion.Integration.ServiceBus.Queues;
using NetFusion.Integration.ServiceBus.Queues.Metadata;
using NetFusion.Integration.ServiceBus.Rpc;
using NetFusion.Integration.ServiceBus.Rpc.Metadata;
using NetFusion.Integration.ServiceBus.Topics;
using NetFusion.Integration.ServiceBus.Topics.Metadata;
using NetFusion.Messaging.Types;

namespace NetFusion.Integration.ServiceBus;

/// <summary>
/// Provides a fluent Api for specifying the supported messaging patterns.
/// Derived from by a microservice and called during bootstrap to define
/// the message types associated with Azure Service Bus namespace entities. 
/// </summary>
public abstract class NamespaceRouter(string busName) : BusRouterBase(busName)
{
    // ----- Queue Message Patterns -----
    
    /// <summary>
    /// Defines a queue to which commands can be published from other microservices
    /// for processing by the defining microservice.
    /// </summary>
    /// <param name="route">The consumer handler and its associated queue metadata.</param>
    /// <typeparam name="TCommand">The type of the command received on the queue.</typeparam>
    protected void DefineQueue<TCommand>(
        Action<CommandRouteWithMeta<TCommand, QueueRouteMeta<TCommand>>> route)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(route);

        var command = new CommandRouteWithMeta<TCommand, QueueRouteMeta<TCommand>>();
        route(command);

        string? queueName = command.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ServiceBusPluginException("Queue name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(command);
        AddBusEntity(new QueueEntity(BusName, queueName, dispatcher));
    }
    
    /// <summary>
    /// Defines a queue to which commands can be published from other microservices for processing
    /// by the defining microservice having an asynchronous response.  The publishing microservice
    /// defines the reply-queue on which the response is delivered.
    /// </summary>
    /// <param name="route">The consumer handler and its associated queue metadata.</param>
    /// <typeparam name="TCommand">The type of the command received on the queue.</typeparam>
    /// <typeparam name="TResult">The type of response sent back to the originating microservice.</typeparam>
    protected void DefineQueueWithResponse<TCommand, TResult>(
        Action<CommandRouteWithMeta<TCommand, TResult, QueueRouteMeta<TCommand>>> route)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(route);

        var command = new CommandRouteWithMeta<TCommand, TResult, QueueRouteMeta<TCommand>>();
        route(command);

        string? queueName = command.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ServiceBusPluginException("Queue name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(command);
        AddBusEntity(new QueueEntity(BusName, queueName, dispatcher));
    }
    
    /// <summary>
    /// Specifies the queue to which a given command should be delivered when sent by the
    /// microservice.  The microservice sending the command does not expect a response.
    /// </summary>
    /// <param name="queueName">The name of the queue the command should be published.</param>
    /// <param name="options">Options determining how the command is published to the queue.</param>
    /// <typeparam name="TCommand">The type of the command when sent should be delivered to queue.</typeparam>
    protected void RouteToQueue<TCommand>(string queueName, Action<QueuePublishOptions>? options = null)
        where TCommand : ICommand
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
        
        var queueReference = new QueueReferenceEntity(typeof(TCommand), BusName, queueName);
        options?.Invoke(queueReference.PublishOptions);
        AddBusEntity(queueReference);
    }
    
    /// <summary>
    /// Specifies the queue to which a given command should be delivered when sent by the
    /// microservice and the reply-queue on which its response should be received.
    /// </summary>
    /// <param name="queueName">The name of the queue the command should be published.</param>
    /// <param name="route">The consumer handler and its associated queue metadata on which it response is routed.</param>
    /// <param name="options">Options determining how the command is published to the queue.</param>
    /// <typeparam name="TCommand">The type of the command when sent should be delivered to the queue.</typeparam>
    /// <typeparam name="TResponse">The type of the commands response to be received on the reply queue.</typeparam>
    protected void RouteToQueueWithResponse<TCommand, TResponse>(string queueName,
        Action<CommandRouteWithMeta<TResponse, QueueRouteMeta<TResponse>>> route,
        Action<QueuePublishOptions>? options = null)
        where TCommand : ICommand<TResponse>
        where TResponse : ICommand
    {
        ArgumentNullException.ThrowIfNull(route);

        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
        
        // Defines queue to which command should be published:
        var queueReference = new QueueReferenceEntity(typeof(TCommand), BusName, queueName);
        options?.Invoke(queueReference.PublishOptions);
        AddBusEntity(queueReference);
        
        // Defines the queue on which the response to the command should be received:
        var command = new CommandRouteWithMeta<TResponse, QueueRouteMeta<TResponse>>();
        route(command);
        
        string? replyQueueName = command.RouteMeta.QueueName;
        if (string.IsNullOrWhiteSpace(replyQueueName))
        {
            throw new ServiceBusPluginException("Queue name must be specified.");
        }

        // Link the two queues:
        queueReference.ReplyQueueName = replyQueueName;
        
        var dispatcher = new MessageDispatcher(command);
        AddBusEntity(new QueueEntity(BusName, replyQueueName, dispatcher));
    }
    
    
    // ----- Topic Message Patterns -----

    /// <summary>
    /// Defines a topic a microservice uses to publish domain-events to notify subscribing
    /// microservices of changes.
    /// </summary>
    /// <param name="meta">Metadata defining how the topic is created.</param>
    /// <typeparam name="TDomainEvent">The type of domain-event associated with the topic.</typeparam>
    protected void DefineTopic<TDomainEvent>(Action<TopicMeta<TDomainEvent>> meta)
        where TDomainEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(meta);

        var topicMeta = new TopicMeta<TDomainEvent>();
        meta(topicMeta);

        if (string.IsNullOrWhiteSpace(topicMeta.TopicName))
        {
            throw new ServiceBusPluginException("Topic name must be specified.");
        }

        var topicEntity = new TopicEntity(typeof(TDomainEvent), BusName, topicMeta.TopicName, topicMeta);
        AddBusEntity(topicEntity);
    }

    /// <summary>
    /// Used by a microservice to subscribe to a topic published to by another microservice
    /// defining the topic.
    /// </summary>
    /// <param name="topicName">The name of the topic to subscribe for notifications.</param>
    /// <param name="route">The route defining the message consumer to be called.</param>
    /// <typeparam name="TDomainEntity">Type of the domain-event received.</typeparam>
    protected void SubscribeToTopic<TDomainEntity>(string topicName, 
        Action<DomainRouteWithMeta<TDomainEntity, SubscriptionMeta<TDomainEntity>>> route)
        where TDomainEntity : IDomainEvent
    {
       if (string.IsNullOrWhiteSpace(topicName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicName));

       ArgumentNullException.ThrowIfNull(route);

        var domainEvent = new DomainRouteWithMeta<TDomainEntity, SubscriptionMeta<TDomainEntity>>();
        route(domainEvent);
        
        string? subscriptionName = domainEvent.RouteMeta.SubscriptionName;
        if (string.IsNullOrWhiteSpace(subscriptionName))
        {
            throw new ServiceBusPluginException("Subscription name must be specified.");
        }
        
        var dispatcher = new MessageDispatcher(domainEvent);
        AddBusEntity(new SubscriptionEntity(BusName, subscriptionName, topicName, dispatcher));
    }
    
    
    // ----- RPC Queue Message Patterns -----

    /// <summary>
    /// Defines a Queue for receiving multiple commands identified by a message-namespace
    /// for which the sending microservice is awaiting a response on a reply-queue.
    /// </summary>
    /// <param name="queueName">The name of the RPC queue to which messages can be sent by other microservices.</param>
    /// <param name="queueMeta">Metadata specifying the characteristics of the created queue.  If not specified,
    /// default settings best suited for a RPC queue are used.</param>
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
    /// Routes a received RPC command to it corresponding consumer handler based on its
    /// associated message-namespace.
    /// </summary>
    /// <param name="queueName">The name of the RPC queue on which the command is received.</param>
    /// <param name="route">Routes the command to its associated consumer.</param>
    /// <param name="messageNamespace">The namespace used to identify the command on the queue and
    /// used to route the command to its consumer's handler.</param>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
    protected void DefineRpcQueueRoute<TCommand, TResult>(
        string queueName,
        Action<CommandRoute<TCommand, TResult>> route,
        string? messageNamespace = null)
    where TCommand : ICommand<TResult>
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

        ArgumentNullException.ThrowIfNull(route);

        // Multiple commands identified by namespace are sent on same queue:
        RpcEntity rpcEntity = ResolveRpcQueue(queueName);
        string rpcMessageNamespace = ResolveMessageNamespace(typeof(TCommand), messageNamespace);
        
        var command = new CommandRoute<TCommand, TResult>();
        route(command);
        
        rpcEntity.AddMessageDispatcher(rpcMessageNamespace, new MessageDispatcher(command));
    }

    private RpcEntity ResolveRpcQueue(string queueName) => BusEntities.OfType<RpcEntity>()
        .FirstOrDefault(ne => ne.EntityName == queueName) ??
        throw new ServiceBusPluginException(
           $"An existing RPC Queue named {queueName} not found within namespace {BusName}.", 
           "RPC_QUEUE_NOTFOUND");
    
    private static string ResolveMessageNamespace(Type messageType, string? messageNamespace) =>
        messageNamespace ?? messageType.GetAttribute<MessageNamespaceAttribute>()?.MessageNamespace ??
            throw new ServiceBusPluginException(
                $"RPC message namespace could not be found for command {messageType}. The namespace must be specified " +
                $"on the command using the {nameof(MessageNamespaceAttribute)} or when mapping the command to the queue.", 
                "RPC_MSG_NS_NOTFOUND");

    /// <summary>
    /// Routes a command to a RPC queue having an associated message-namespace.
    /// </summary>
    /// <param name="queueName">The name of the RPC queue.</param>
    /// <param name="publishOptions">Options used when publishing the command to the queue.</param>
    /// <param name="processingOptions">The process options to used on the RPC reply queue.</param>
    /// <param name="messageNamespace">The unique namespace name associated with the command.</param>
    /// <typeparam name="TCommand">The type of the command to be sent to the RPC queue.</typeparam>
    protected void RouteToRpcQueue<TCommand>(string queueName,
        Action<RpcPublishOptions>? publishOptions = null, 
        Action<RpcProcessingOptions>? processingOptions = null,
        string? messageNamespace = null)
        where TCommand : ICommand
    {
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

        var rpcEntity = BusEntities.OfType<RpcReferenceEntity>().FirstOrDefault(re => re.EntityName == queueName);
        if (rpcEntity == null)
        {
            rpcEntity = new RpcReferenceEntity(BusName, queueName);
            publishOptions?.Invoke(rpcEntity.PublishOptions);
            processingOptions?.Invoke(rpcEntity.ProcessingOptions);
            AddBusEntity(rpcEntity);
        }

        if (publishOptions != null || processingOptions != null)
        {
            throw new ServiceBusPluginException(
                "When routing a command to an RPC Queue, options can only be set on the first routed command");
        }
        
        var commandType = typeof(TCommand);
        var rpcMessageNamespace = ResolveMessageNamespace(commandType, messageNamespace);
        
        rpcEntity.AddCommandMessageNamespace(commandType, rpcMessageNamespace);
    }
}