namespace NetFusion.Azure.Messaging.Subscriber
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class from which an application specific class derives to specify
    /// overrides for a Topic's subscriptions that are defined within the code.
    /// The host application registers an instance of this class and maps
    /// the host specific subscription that should be used.
    /// </summary>
    public class SubscriptionSettingsBase : ISubscriptionSettings
    {
        private readonly Dictionary<string, List<SubscriptionMapping>> _mappings;

        public SubscriptionSettingsBase()
        {
            _mappings = new Dictionary<string, List<SubscriptionMapping>>();
        }

        public string GetMappedSubscription(string namespaceName, SubscriptionMapping mapping)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace name not specified.", nameof(namespaceName));

            if (!_mappings.TryGetValue(namespaceName, out List<SubscriptionMapping> nsMappings))
            {
                return null;
            }

            var subscriptionMapping = nsMappings.FirstOrDefault(m =>
                m.TopicName == mapping.TopicName && m.SubscriptionName == mapping.SubscriptionName);
            
            return subscriptionMapping?.MappedSubscriptionName;
        }

        public virtual Task ConfigureSettings()
        {
            return Task.CompletedTask;
        }

        public virtual Task CleanupSettings()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes an action against all defined mappings.
        /// </summary>
        /// <param name="action">The action passed a configured mapping.</param>
        protected void ForeachMapping(Action<SubscriptionMapping> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            foreach (SubscriptionMapping mapping in _mappings.Values.SelectMany(m => m))
            {
                action(mapping);
            }
        }

        /// <summary>
        /// Adds a mapping for a topic's subscription defined within code to an actual host
        /// specific created subscription.
        /// </summary>
        /// <param name="namespaceName">The namespace defining the topic.</param>
        /// <param name="mappings">The mapping settings.</param>
        public void AddMapping(string namespaceName, params SubscriptionMapping[] mappings)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace name not specified.", nameof(namespaceName));
            
            if (mappings == null) throw new ArgumentNullException(nameof(mappings));
            
            if (! _mappings.TryGetValue(namespaceName, out List<SubscriptionMapping> namespaceMappings))
            {
                namespaceMappings = new List<SubscriptionMapping>();
                _mappings.Add(namespaceName, namespaceMappings);
            }

            foreach (var mapping in mappings)
            {
                AssertValidMapping(namespaceName, namespaceMappings, mapping);
                namespaceMappings.Add(mapping);
            }
        }

        private static void AssertValidMapping(string namespaceName, IEnumerable<SubscriptionMapping> namespaceMappings,
            SubscriptionMapping mapping)
        {
            var matchingMapping = namespaceMappings.FirstOrDefault(m =>
                m.TopicName == mapping.TopicName &&
                m.SubscriptionName == mapping.SubscriptionName);

            if (matchingMapping != null)
            {
                throw new InvalidOperationException(
                    $"A mapping for the Subscription: {matchingMapping.SubscriptionName} on Topic: {matchingMapping.TopicName} " +
                    $"defined within the Namespace: {namespaceName} is already mapped to Subscription: {matchingMapping.MappedSubscriptionName}");
            }
        }
    }

    /// <summary>
    /// Null implementation that always returns a non-found mapping.
    /// </summary>
    internal class NullSubscriptionSettings : ISubscriptionSettings
    {
        public string GetMappedSubscription(string namespaceName, SubscriptionMapping mapping)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace name not specified.", nameof(namespaceName));
            
            return null;
        }

        public Task ConfigureSettings()
        {
            return Task.CompletedTask;
        }

        public Task CleanupSettings()
        {
            return Task.CompletedTask;
        }
    }
}