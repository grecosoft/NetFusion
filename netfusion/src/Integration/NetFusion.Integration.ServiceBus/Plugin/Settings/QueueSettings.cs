namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

public class QueueSettings
{
    /// <summary>
    /// Gets or sets the number of messages that will be eagerly requested from Queues
    /// or Subscriptions and queued locally, intended to help maximize throughput by
    /// allowing the processor to receive from a local cache rather than waiting on a
    /// service request.  The default value is 0.
    /// </summary>
    public int? PrefetchCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent calls to the message handler the
    /// processor should initiate.  The default value is 1.
    /// </summary>
    public int? MaxConcurrentCalls { get; set; }
    
    /// <summary>
    /// Duration of a peek lock receive. i.e., the amount of time that the message is locked by a given receiver so that
    /// no other receiver receives the same message.
    /// </summary>
    public int? LockDurationInSeconds { get; set; }
    
    /// <summary>
    /// Indicates whether this queue has dead letter support when a message expires.
    /// </summary>
    /// <remarks>If true, the expired messages are moved to dead-letter subqueue. Default value is false.</remarks>
    public bool? DeadLetteringOnMessageExpiration { get; set; }
    
    /// <summary>
    /// The maximum delivery count of a message before it is dead-lettered.
    /// </summary>
    /// <remarks>The delivery count is increased when a message is received in <see cref="ServiceBusReceiveMode.PeekLock"/> mode
    /// and didn't complete the message before the message lock expired.
    /// Default value is 10. Minimum value is 1.</remarks>
    public int? MaxDeliveryCount { get; set; }
    
    /// <summary>
    /// The name of the recipient entity to which all the dead-lettered messages of this queue are forwarded to.
    /// </summary>
    /// <remarks>If set, user cannot manually receive dead-lettered messages from this queue. The destination
    /// entity must already exist.</remarks>
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    
    /// <summary>
    /// The maximum size of the queue in megabytes, which is the size of memory allocated for the queue.
    /// </summary>
    /// <remarks>Default value is 1024.</remarks>
    public long? MaxSizeInMegabytes { get; set; }
}