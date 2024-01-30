namespace NetFusion.Integration.RabbitMQ.Queues.Metadata;

/// <summary>
/// Contains metadata describing a broker queue.
/// </summary>
public class QueueMeta
{
    /// <summary>
    /// The name of the queue. 
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// Indicates that any unacknowledged messages should be sent to the dead letter exchange
    /// and saved to any bound queues for future processing. 
    /// </summary>
    public string? DeadLetterExchangeName { get; set; }

    /// <summary>
    /// Indicates that the queue should survive a server restart.
    /// </summary>
    public bool IsDurable { get; set; } 

    /// <summary>
    /// Determines if multiple clients can monitor queue.
    /// </summary>
    public bool IsExclusive { get; set; } 

    /// <summary>
    /// The queue is automatically deleted when the last consumer un-subscribes.
    /// If you need a temporary queue used only by one consumer, combine auto-delete
    /// with exclusive.  When the consumer disconnects, the queue will be removed.
    /// </summary>
    public bool IsAutoDelete { get; set; }
    
    /// <summary>
    /// Determines the maximum message priority that the queue should support.
    /// </summary>
    public int? MaxPriority { get; set; }

    /// <summary>
    /// How long in milliseconds a message should remain on the queue before it is discarded.
    /// </summary>
    public int? PerQueueMessageTtl { get; set;}

    /// <summary>
    /// This is the number of messages that will be delivered by RabbitMQ before an ack is sent by EasyNetQ.
    /// Set to 0 for infinite prefetch (not recommended). Set to 1 for fair work balancing among a farm of consumers.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 1;

    public int Priority { get; set; }
}

public class QueueMeta<TMessage> : QueueMeta, IRouteMeta<TMessage>
    where TMessage : IMessage;