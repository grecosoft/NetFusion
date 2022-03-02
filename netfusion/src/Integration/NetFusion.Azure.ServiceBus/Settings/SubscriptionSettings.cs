using System.Collections.Generic;
using NetFusion.Base.Validation;

namespace NetFusion.Azure.ServiceBus.Settings
{
    /// <summary>
    /// Settings for a subscription to a Service Bus entity. If not specified,
    /// the default settings specified within code will be used.
    /// </summary>
    public class SubscriptionSettings : IValidatableType
    {
        /// <summary>
        /// Gets or sets the number of messages that will be eagerly requested from Queues
        ///  or Subscriptions and queued locally, intended to help maximize throughput by
        ///  allowing the processor to receive from a local cache rather than waiting on a
        ///  service request.  The default value is 0.
        /// </summary>
        public int? PrefetchCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of concurrent calls to the message handler the
        /// processor should initiate.  The default value is 1.
        /// </summary>
        public int? MaxConcurrentCalls { get; set; }

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
}
