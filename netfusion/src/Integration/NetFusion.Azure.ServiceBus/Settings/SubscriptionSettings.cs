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
        public int? PrefetchCount { get; set; }
        public int? MaxConcurrentCalls { get; set; }

        public Dictionary<string, RuleSettings> Rules { get; set; } = new Dictionary<string, RuleSettings>();
        
        public void Validate(IObjectValidator validator)
        {
            validator.Verify(PrefetchCount == null || PrefetchCount >= 0, 
                "PrefetchCount if specified must be greater than or equal zero.");
            
            validator.Verify(MaxConcurrentCalls == null || MaxConcurrentCalls > 0, 
                "MaxConcurrentCalls if specified must be greater than zero.");

            validator.AddChildren(Rules.Values);

        }
    }
}
