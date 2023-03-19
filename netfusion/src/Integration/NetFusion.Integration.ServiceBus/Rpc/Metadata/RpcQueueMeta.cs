namespace NetFusion.Integration.ServiceBus.Rpc.Metadata;

/// <summary>
/// Metadata used to define a queue on which a microservice receives
/// RPC style of messages.
/// </summary>
public class RpcQueueMeta : IRpcQueueMeta
{
    public string QueueName { get; }
    public RpcProcessingOptions ProcessingOptions { get; }
    
    public RpcQueueMeta(string queueName)
    {
        QueueName = queueName;
        ProcessingOptions = new RpcProcessingOptions();
    }
    
    public TimeSpan? LockDuration { get; set; }
    public int? MaxDeliveryCount { get; set; }
    public long? MaxSizeInMegabytes { get; set; }
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
}