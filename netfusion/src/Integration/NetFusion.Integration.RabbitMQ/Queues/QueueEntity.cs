using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.RabbitMQ.Queues.Strategies;

namespace NetFusion.Integration.RabbitMQ.Queues;

/// <summary>
/// Defines a queue defined by a microservice to with other microservices
/// send commands for processing.
/// </summary>
public class QueueEntity : BusEntity
{
    public QueueMeta QueueMeta { get; }
    public MessageDispatcher MessageDispatcher { get; }
    
    public QueueEntity(string busName, string queueName, MessageDispatcher dispatcher) :
        base(busName, queueName)
    {
        QueueMeta = dispatcher.RouteMeta as QueueMeta ??
                    throw new NullReferenceException("Queue metadata not specified");

        MessageDispatcher = dispatcher;

        AddStrategies(
            new QueueCreationStrategy(this));
    }
    
    public override IEnumerable<MessageDispatcher> Dispatchers => new[] { MessageDispatcher };

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "CommandType", MessageDispatcher.MessageType.Name },
        { "Consumer", MessageDispatcher.ConsumerType.Name },
        { "Handler", MessageDispatcher.MessageHandlerMethod.Name },
        { "BusName", BusName },
        { "QueueName", QueueMeta.QueueName ?? string.Empty },
        { "IsDurable", QueueMeta.IsDurable.ToString() },
        { "IsAutoDelete", QueueMeta.IsAutoDelete.ToString() },
        { "IsExclusive", QueueMeta.IsExclusive.ToString() },
        { "DeadLetterExchangeName", QueueMeta.DeadLetterExchangeName ?? string.Empty },
        { "Priority", QueueMeta.Priority.ToString() },
        { "MaxPriority", QueueMeta.MaxPriority?.ToString() ?? string.Empty },
        { "PerQueueMessageTtl", QueueMeta.PerQueueMessageTtl?.ToString() ?? string.Empty },
        { "PrefetchCount", QueueMeta.PrefetchCount.ToString() },
    };
}