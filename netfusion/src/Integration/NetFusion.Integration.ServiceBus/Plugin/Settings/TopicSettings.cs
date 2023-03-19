namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

public class TopicSettings
{
    /// <summary>
    /// The maximum size of the topic in megabytes, which is the size of memory allocated for the topic.
    /// </summary>
    public long? MaxSizeInMegabytes { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum message size, in kilobytes, for messages sent to this topic.
    /// This feature is only available when using a Premium namespace and service version "2021-05" or higher.
    /// <seealso href="https://docs.microsoft.com/azure/service-bus-messaging/service-bus-premium-messaging"/>
    /// </summary>
    public long? MaxMessageSizeInKilobytes { get; set; }
    
    /// <summary>
    /// The default time to live value for the messages. This is the duration after which the message expires,
    /// starting from when the message is sent to Service Bus.
    /// </summary>
    public int? DefaultMessageTimeToLiveInSeconds { get; set; }
}