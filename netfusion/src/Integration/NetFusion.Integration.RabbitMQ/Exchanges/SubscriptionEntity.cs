using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Exchanges.Strategies;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;

namespace NetFusion.Integration.RabbitMQ.Exchanges;

/// <summary>
/// Defines a subscription to a queue, on an exchange, to which the microservice
/// wants to receive domain-events published by another service owning the exchange.
/// </summary>
public class SubscriptionEntity : BusEntity
{
    public string ExchangeName { get; }
    public string[] RouteKeys { get; }
    public bool IsPerServiceInstance { get; }
    public QueueMeta QueueMeta { get; }
    public MessageDispatcher MessageDispatcher { get; }

    public SubscriptionEntity(string busName, string queueName,
        string exchangeName,
        string[] routeKeys,
        MessageDispatcher dispatcher) : base(busName, queueName)
    {
        ExchangeName = exchangeName;
        RouteKeys = routeKeys;
        MessageDispatcher = dispatcher;

        QueueMeta = dispatcher.RouteMeta as QueueMeta ??
                    throw new NullReferenceException("Queue metadata not specified");

        AddStrategies(new ExchangeSubscriptionStrategy(this));
    }

    public override IEnumerable<MessageDispatcher> Dispatchers => new[] { MessageDispatcher };

    public SubscriptionEntity(string busName, string queueName,
        string exchangeName,
        bool isPerServiceInstance,
        MessageDispatcher dispatcher) : this (busName, queueName, exchangeName, Array.Empty<string>(), dispatcher)
    {
        IsPerServiceInstance = isPerServiceInstance;
    }

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "DomainEventType", MessageDispatcher.MessageType.Name },
        { "Consumer", MessageDispatcher.ConsumerType.Name },
        { "Handler", MessageDispatcher.MessageHandlerMethod.Name },
        { "BusName", BusName },
        { "ExchangeName", ExchangeName },
        { "QueueName", QueueMeta.QueueName },
        { "RouteKeys", string.Join(", ", RouteKeys) },
        { "IsDurable", QueueMeta.IsDurable.ToString() },
        { "IsAutoDelete", QueueMeta.IsAutoDelete.ToString() },
        { "IsExclusive", QueueMeta.IsExclusive.ToString() },
        { "DeadLetterExchangeName", QueueMeta.DeadLetterExchangeName },
        { "Priority", QueueMeta.Priority.ToString() },
        { "MaxPriority", QueueMeta.MaxPriority?.ToString() },
        { "PerQueueMessageTtl", QueueMeta.PerQueueMessageTtl?.ToString() },
        { "PrefetchCount", QueueMeta.PrefetchCount.ToString() },
    };
}