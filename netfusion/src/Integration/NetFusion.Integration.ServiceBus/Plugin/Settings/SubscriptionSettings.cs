using NetFusion.Common.Base.Validation;

namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

/// <summary>
/// Settings for a subscription to a Service Bus entity. If not specified,
/// the default settings specified within code will be used.
/// </summary>
public class SubscriptionSettings : IValidatableType
{
    /// <summary>
    /// Gets or sets the number of messages that will be eagerly requested from Queues
    /// or Subscriptions and queued locally, intended to help maximize throughput by
    /// allowing the processor to receive from a local cache rather than waiting on a
    /// service request.  The default value is 0.
    /// </summary>
    public int? PrefetchCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent calls to the message handler the
    /// processor should initiate.  The default value is 1.
    /// </summary>
    public int? MaxConcurrentCalls { get; set; }
    
    /// <summary>
    /// Duration of a peek lock receive. i.e., the amount of time that the message is locked by a given receiver so that
    /// no other receiver receives the same message.
    /// </summary>
    /// <remarks>Max value is 5 minutes. Default value is 60 seconds.</remarks>
    public int? LockDurationInSeconds { get; set; }
    
    /// <summary>
    /// The maximum delivery count of a message before it is dead-lettered.
    /// </summary>
    public int? MaxDeliveryCount { get; set; }
    
    /// <summary>
    /// The default time to live value for the messages. This is the duration after which the message expires, starting from when
    /// the message is sent to Service Bus.
    /// </summary>
    public int? DefaultMessageTimeToLiveInSeconds { get; set; }
    
    /// <summary>
    /// Indicates whether this subscription has dead letter support when a message expires.
    /// </summary>
    public bool? DeadLetteringOnMessageExpiration { get; set; }
    
    /// <summary>
    /// The name of the recipient entity to which all the dead-lettered messages of this subscription are forwarded to.
    /// </summary>
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    
    /// <summary>
    /// indicates whether messages need to be forwarded to dead-letter sub queue when subscription rule evaluation fails.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool? EnableDeadLetteringOnFilterEvaluationExceptions { get; set; }

    /// <summary>
    /// List of filters determining if a topic subscription should be delivered
    /// to a subscription.
    /// </summary>
    public Dictionary<string, RuleSettings> Rules { get; set; } = new();

    public void Validate(IObjectValidator validator)
    {
        validator.Verify(PrefetchCount is null or >= 0,
            "PrefetchCount if specified must be greater than or equal zero.");

        validator.Verify(MaxConcurrentCalls is null or > 0,
            "MaxConcurrentCalls if specified must be greater than zero.");

        validator.AddChildren(Rules.Values);
    }
}