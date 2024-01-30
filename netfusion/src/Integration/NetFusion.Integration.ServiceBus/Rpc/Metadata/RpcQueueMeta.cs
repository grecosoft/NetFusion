namespace NetFusion.Integration.ServiceBus.Rpc.Metadata;

/// <summary>
/// Metadata used to define a queue on which a microservice receives
/// RPC style of messages.
/// </summary>
public class RpcQueueMeta(string queueName) : IRpcQueueMeta
{
    public string QueueName { get; } = queueName;
    public RpcProcessingOptions ProcessingOptions { get; } = new();

    public TimeSpan? LockDuration { get; set; }
    public int? MaxDeliveryCount { get; set; }
    public long? MaxSizeInMegabytes { get; set; }
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
}