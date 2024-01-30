using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.ServiceBus.Internal;

namespace NetFusion.Integration.ServiceBus.Topics.Metadata;

public abstract class SubscriptionMeta
{
    private readonly Dictionary<string, CreateRuleOptions> _rules = new();

    /// <summary>
    /// Indicates that each instance of the same running microservice will receive the
    /// message when sent to a topic.  If no instances of a given microservice are running,
    /// and delivered messages are ignored.  This can be used to notify all microservice
    /// instances of the same type running within a cluster.
    /// </summary>
    public bool IsPerServiceInstance { get; set; } = false;

    /// <summary>
    /// Options to use when processing sent messages from the service-bus.
    /// </summary>
    public ServiceBusProcessorOptions ProcessingOptions { get; } = new();

    /// <summary>
    /// The roles used to determine if a message sent to the topic matches
    /// the criteria to be delivered to the subscription.
    /// </summary>
    public IEnumerable<CreateRuleOptions> RuleOptions => _rules.Values;
    
    public string? SubscriptionName { get; set; }
    
    /// <summary>
    /// Duration of a peek lock receive. i.e., the amount of time that the message is locked by a given receiver so that
    /// no other receiver receives the same message.
    /// </summary>
    /// <remarks>Max value is 5 minutes. Default value is 60 seconds.</remarks>
    public TimeSpan? LockDuration { get; set; }
    
    /// <summary>
    /// This indicates whether the subscription supports the concept of session. Sessionful-messages follow FIFO ordering.
    /// </summary>
    public bool? RequiresSession { get; set; }
    
    /// <summary>
    /// The default time to live value for the messages. This is the duration after which the message expires, starting from when
    /// the message is sent to Service Bus.
    /// </summary>
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
    
    /// <summary>
    /// The <see cref="TimeSpan"/> idle interval after which the subscription is automatically deleted.
    /// </summary>
    public TimeSpan? AutoDeleteOnIdle { get; set; }
    
    /// <summary>
    /// Indicates whether this subscription has dead letter support when a message expires.
    /// </summary>
    public bool? DeadLetteringOnMessageExpiration { get; set; }
    
    /// <summary>
    /// indicates whether messages need to be forwarded to dead-letter sub queue when subscription rule evaluation fails.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool? EnableDeadLetteringOnFilterEvaluationExceptions { get; set; }
    
    /// <summary>
    /// The maximum delivery count of a message before it is dead-lettered.
    /// </summary>
    public int? MaxDeliveryCount { get; set; }
    
    /// <summary>
    /// Indicates whether server-side batched operations are enabled.
    /// </summary>
    public bool? EnableBatchedOperations { get; set; }
    
    /// <summary>
    /// The name of the recipient entity to which all the messages sent to the subscription are forwarded to.
    /// </summary>
    public string? ForwardTo { get; set; }
    
    /// <summary>
    /// The name of the recipient entity to which all the dead-lettered messages of this subscription are forwarded to.
    /// </summary>
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    
    /// <summary>
    /// Called to add a rule to a subscription used to determine if messages apply.
    /// </summary>
    /// <param name="name">The name of the role.</param>
    /// <param name="filter">Reference to the created filter.</param>
    /// <param name="config">Delegate called used to apply additional settings.</param>
    public void AddRule(string name, RuleFilter filter, Action<CreateRuleOptions>? config = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name not specified.", nameof(name));

        ArgumentNullException.ThrowIfNull(filter);

        if (_rules.ContainsKey(name))
        {
            throw new ServiceBusPluginException(
                $"Subscription {SubscriptionName} already has a filter named {name} defined.", "DUPLICATE_FILTER");
        }
            
        var rule = _rules[name] = new CreateRuleOptions(name, filter);
        config?.Invoke(rule);
    }
    
}

/// <summary>
/// Metadata defining how the consumer is subscribed to the subscription.
/// </summary>
/// <typeparam name="TDomainEvent">The domain-event type associated with subscription.</typeparam>
public class SubscriptionMeta<TDomainEvent> : SubscriptionMeta, IRouteMeta<TDomainEvent>
    where TDomainEvent : IDomainEvent;