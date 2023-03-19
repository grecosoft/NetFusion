using Azure.Messaging.ServiceBus;

namespace NetFusion.Integration.ServiceBus.Queues.Metadata;

public class QueueRouteMeta
{
    public string? QueueName { get; set; }
    public TimeSpan? LockDuration { get; set; }
    public bool? RequiresSession { get; set; }
    public bool? DeadLetteringOnMessageExpiration { get; set; }
    public int? MaxDeliveryCount { get; set; }
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    public long? MaxSizeInMegabytes { get; set; }
    public bool? RequiresDuplicateDetection { get; set; }
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
    public TimeSpan? DuplicateDetectionHistoryTimeWindow { get; set; }
    public bool? EnableBatchedOperations { get; set; }
    public bool? EnablePartitioning { get; set; }
    public ServiceBusProcessorOptions ProcessingOptions { get; } = new();
}

public class QueueRouteMeta<TCommand> : QueueRouteMeta,
    IRouteMeta<TCommand>
    where TCommand : ICommand
{
    
}