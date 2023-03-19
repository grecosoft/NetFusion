using Azure.Messaging.ServiceBus;

namespace NetFusion.Integration.ServiceBus.Topics.Metadata;

public abstract class TopicMeta
{
    public string? TopicName { get; set; }
    
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
    /// This value indicates if the topic requires guard against duplicate messages. If true, duplicate messages having same
    /// <see cref="MessageId"/> and sent to topic within duration of <see cref="DuplicateDetectionHistoryTimeWindow"/>
    /// will be discarded.
    /// </summary>
    public bool? RequiresDuplicateDetection { get; set; }
    
    /// <summary>
    /// The default time to live value for the messages. This is the duration after which the message expires,
    /// starting from when the message is sent to Service Bus.
    /// </summary>
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
    
    /// <summary>
    /// The <see cref="TimeSpan"/> idle interval after which the topic is automatically deleted.
    /// </summary>
    public TimeSpan? AutoDeleteOnIdle { get; set; }
    
    /// <summary>
    /// The <see cref="TimeSpan"/> duration of duplicate detection history that is maintained by the service.
    /// </summary>
    public TimeSpan? DuplicateDetectionHistoryTimeWindow { get; set; }
    
    /// <summary>
    /// Defines whether ordering needs to be maintained. If true, messages sent to topic will be
    /// forwarded to the subscription in order.
    /// </summary>
    public bool? SupportOrdering { get; set; }
    
    /// <summary>
    /// Indicates whether server-side batched operations are enabled.
    /// </summary>
    public bool? EnableBatchedOperations { get; set; }
    
    /// <summary>
    /// Indicates whether the topic is to be partitioned across multiple message brokers.
    /// </summary>
    public bool? EnablePartitioning { get; set; }
    
    /// <summary>
    /// Determines if the message meets the criteria required to be published to the topic.
    /// </summary>
    /// <param name="message">The message being published.</param>
    /// <returns>True if the message can be published.  Otherwise, false.</returns>
    internal abstract bool DoesMessageApply(IMessage message);
    
    /// <summary>
    /// Allows message properties to be set for use by subscription filters.
    /// </summary>
    /// <param name="serviceBusMessage">The created service bus message.</param>
    /// <param name="message">The domain-event being published.</param>
    internal abstract void ApplyMessageProperties(ServiceBusMessage serviceBusMessage, IMessage message);
}

/// <summary>
/// Metadata defining how a topic is created to which domain-events
/// can be published.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event associated with the topic.</typeparam>
public class TopicMeta<TDomainEvent> : TopicMeta
    where TDomainEvent : IDomainEvent
{
    private Predicate<TDomainEvent> _messagePredicate = _ => true;
    private Action<ServiceBusMessage, TDomainEvent> _messagePropAction = (_, _) => { };

    /// <summary>
    /// Allows messages properties to be set form the domain-event used by
    /// subscribers to filter delivered messages.
    /// </summary>
    /// <param name="messages">Delegate passed the created service-bus message
    /// and the original domain-event. </param>
    public void SetMessageProperties(Action<ServiceBusMessage, TDomainEvent> messages)
    {
        _messagePropAction = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    /// <summary>
    /// Allows specifying a predicate used to determine if the published domain-event
    /// matches the criteria required to be delivered to the topic.
    /// </summary>
    /// <param name="domainEvent">The domain-event to be evaluated.</param>
    public void WhenDomainEvent(Predicate<TDomainEvent> domainEvent)
    {
        _messagePredicate = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
    }

    internal override void ApplyMessageProperties(ServiceBusMessage serviceBusMessage, IMessage message) =>
        _messagePropAction(serviceBusMessage, (TDomainEvent)message);

    internal override bool DoesMessageApply(IMessage message) => _messagePredicate((TDomainEvent)message);
}