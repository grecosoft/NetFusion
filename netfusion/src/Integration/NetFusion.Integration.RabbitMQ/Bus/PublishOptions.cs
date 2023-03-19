using NetFusion.Common.Base;

namespace NetFusion.Integration.RabbitMQ.Bus;

// Options used when publishing a message.
public class PublishOptions
{
    public string ContentType { get; set; } = ContentTypes.Json;
    public byte? Priority { get; set; }
    public bool IsPersistent { get; set; }
    public bool IsMandatory { get; set; }
}