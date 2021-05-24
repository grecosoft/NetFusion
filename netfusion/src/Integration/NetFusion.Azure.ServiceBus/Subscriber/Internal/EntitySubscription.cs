using System;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Settings;
using NetFusion.Messaging.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber.Internal
{
    /// <summary>
    /// Contains information for a subscription to a namespace entity.
    /// </summary>
    public abstract class EntitySubscription
    {
        /// <summary>
        /// The namespace as specified within the application settings.
        /// </summary>
        public string NamespaceName { get; }
        
        /// <summary>
        /// The name of the Service Bus entity to subscribe.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Associated subscription object that can be used to override the default values.
        /// </summary>
        public ServiceBusProcessorOptions Options { get; }

        /// <summary>
        /// The namespace as specified within the application settings.
        /// </summary>
        /// <param name="namespaceName">The namespace as specified ied within the application settings.</param>
        /// <param name="entityName">The name of the entity to subscribe.</param>
        protected EntitySubscription(string namespaceName, string entityName)
        {
            NamespaceName = namespaceName;
            EntityName = entityName;
            SettingsKey = entityName;
            Options = new ServiceBusProcessorOptions();
        }

        // Value corresponding to the key used to find setting overrides specified within application settings.
        internal string SettingsKey { get; set; }
        
        // The dispatch information for the method handler to be called when message arrives.
        internal MessageDispatchInfo DispatchInfo { get; set; }
        
        // Strategy knowing how to subscribe to a specific type of namespace entity.
        internal ISubscriptionStrategy SubscriptionStrategy { get; set; }
        
        // The created Azure Service Processor monitoring the namespace entity for messages.
        internal ServiceBusProcessor Processor { get; private set; }

        internal void ProcessedBy(ServiceBusProcessor processor)
        {
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        internal virtual void ApplySettings(SubscriptionSettings settings)
        {
            Options.PrefetchCount = settings.PrefetchCount ?? Options.PrefetchCount;
            Options.MaxConcurrentCalls = settings.MaxConcurrentCalls ?? Options.MaxConcurrentCalls;
        }
    }
}