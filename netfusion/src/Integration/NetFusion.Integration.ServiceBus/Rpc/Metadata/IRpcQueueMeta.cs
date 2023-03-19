namespace NetFusion.Integration.ServiceBus.Rpc.Metadata;

/// <summary>
/// Metadata used to define a queue on which a microservice receives
/// RPC style of messages.
/// </summary>
public interface IRpcQueueMeta
{
    /// <summary>
    /// Options used when processing messages from the queue.
    /// </summary>
    public RpcProcessingOptions ProcessingOptions { get; }
    
    /// <summary>
    /// Duration of a peek lock receive. i.e., the amount of time that the message is locked by a given receiver so that
    /// no other receiver receives the same message.
    /// </summary>
    /// <remarks>Max value is 5 minutes. Default value is 60 seconds.</remarks>
    public TimeSpan? LockDuration { get; set; }
    
    /// <summary>
    /// The maximum delivery count of a message before it is dead-lettered.
    /// </summary>
    /// <remarks>The delivery count is increased when a message is received in PeekLock mode
    /// and didn't complete the message before the message lock expired.
    /// Default value is 10. Minimum value is 1.</remarks>
    public int? MaxDeliveryCount { get; set; }
    
    /// <summary>
    /// The maximum size of the queue in megabytes, which is the size of memory allocated for the queue.
    /// </summary>
    /// <remarks>Default value is 1024.</remarks>
    public long? MaxSizeInMegabytes { get; set; }
    
    /// <summary>
    /// The default time to live value for the messages. This is the duration after which the message expires, starting from when
    /// the message is sent to Service Bus. </summary>
    /// <remarks>
    /// This is the default value used when TimeToLive is not set on a
    ///  message itself. Messages older than their TimeToLive value will expire and no longer be retained in the message store.
    ///  Subscribers will be unable to receive expired messages.
    ///  Default value is <see cref="TimeSpan.MaxValue"/>.
    ///  </remarks>
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
}