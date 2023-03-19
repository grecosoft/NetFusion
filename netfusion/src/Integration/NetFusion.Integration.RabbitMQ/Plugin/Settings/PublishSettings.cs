namespace NetFusion.Integration.RabbitMQ.Plugin.Settings;

public class PublishSettings
{
    public string? ContentType { get; set; }
    public byte? Priority { get; set; }
    public bool? IsPersistent { get; set; }
    public bool? IsMandatory { get; set; }
}