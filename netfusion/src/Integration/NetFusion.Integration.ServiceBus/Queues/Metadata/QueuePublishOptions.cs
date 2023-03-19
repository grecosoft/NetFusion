using NetFusion.Common.Base;

namespace NetFusion.Integration.ServiceBus.Queues.Metadata;

/// <summary>
/// Options used when a command is published to the queue.
/// </summary>
public class QueuePublishOptions
{
    public string ContentType { get; set; } = ContentTypes.Json;
}