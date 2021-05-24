using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Azure.Messaging.ServiceBus;
using NetFusion.Base.Validation;

namespace NetFusion.Azure.ServiceBus.Settings
{
    /// <summary>
    /// Settings for a specific Azure Service Bus namespace.
    /// </summary>
    public class NamespaceSettings : IValidatableType
    {
        /// <summary>
        /// The name of the namespace.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Azure Service Bus Namespace required.")]
        public string Name { get; set; }
        
        /// <summary>
        /// The connection string used to connect to the namespace.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Azure Service Bus Connection required.")]
        public string ConnString { get; set; }
        
        /// <summary>
        /// The type of protocol and transport that will be used for communicating with the Service Bus service.
        /// </summary>
        public ServiceBusTransportType? TransportType { get; set; }

        /// <summary>
        /// Setting to be used to influence how dropped connections are retried.
        /// </summary>
        public RetrySettings RetrySettings { get; set; }

        /// <summary>
        /// Setting overrides for any defined entity subscriptions.
        /// </summary>
        public Dictionary<string, SubscriptionSettings> Subscriptions { get; set; } = new();

        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Subscriptions.Values);
        }
    }
}