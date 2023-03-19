using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Queues.Metadata;
using NetFusion.Integration.ServiceBus.Queues.Strategies;

namespace NetFusion.Integration.ServiceBus.Queues;

/// <summary>
/// Allows a microservice to define a Queue namespace entity to which commands can be sent
/// by other microservices for processing.
/// </summary>
internal class QueueEntity : BusEntity
{
    public QueueRouteMeta QueueMeta { get; }
    private readonly MessageDispatcher _messageDispatcher;
    
    public QueueEntity(string namespaceName, string queueName, MessageDispatcher dispatcher) 
        : base(namespaceName, queueName)
    {
        _messageDispatcher = dispatcher;
        
        QueueMeta = dispatcher.RouteMeta as QueueRouteMeta ?? 
            throw new NullReferenceException("Queue metadata not specified");
        
        AddStrategies(
            new QueueCreationStrategy(this),
            new QueueSubscriptionStrategy(this, dispatcher));
    }
    
    public override IEnumerable<MessageDispatcher> Dispatchers => new[] { _messageDispatcher };

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "CommandType", _messageDispatcher.MessageType.Name },
        { "Consumer", _messageDispatcher.ConsumerType.Name },
        { "Handler", _messageDispatcher.MessageHandlerMethod.Name },
        { "BusName", BusName },
        { "QueueName", QueueMeta.QueueName },
        { "LockDuration", QueueMeta.LockDuration?.ToString() },
        { "RequiresSession", QueueMeta.RequiresSession.ToString() },
        { "DeadLetteringOnMessageExpiration", QueueMeta.DeadLetteringOnMessageExpiration?.ToString() },
        { "MaxDeliveryCount", QueueMeta.MaxDeliveryCount?.ToString() },
        { "ForwardDeadLetteredMessagesTo", QueueMeta.ForwardDeadLetteredMessagesTo },
        { "MaxSizeInMegabytes", QueueMeta.MaxSizeInMegabytes?.ToString() },
        { "RequiresDuplicateDetection", QueueMeta.RequiresDuplicateDetection?.ToString() },
        { "DefaultMessageTimeToLive", QueueMeta.DefaultMessageTimeToLive.ToString() },
        { "DuplicateDetectionHistoryTimeWindow", QueueMeta.DuplicateDetectionHistoryTimeWindow?.ToString() },
        { "EnableBatchedOperations", QueueMeta.EnableBatchedOperations?.ToString() },
        { "EnablePartitioning", QueueMeta.EnablePartitioning.ToString()  },
    };
}