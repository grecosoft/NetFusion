using NetFusion.Common.Base;

namespace NetFusion.Integration.ServiceBus.Topics.Metadata;

/// <summary>
/// Default options used when publishing a domain-event to a topic.
/// </summary>
public class TopicPublishOptions
{
    /// <summary>
    /// The content-type to use when serializing the domain-event.
    /// </summary>
    public string ContentType { get; set; } = ContentTypes.Json;
}