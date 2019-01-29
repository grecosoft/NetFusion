using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Azure.Messaging.Subscriber
{
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

        public string GetMappedSubscription(SubscriptionMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            if (!_mappings.TryGetValue(mapping.NamespaceName, out List<SubscriptionMapping> nsMappings))
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
        /// Reference to all of the 
        /// </summary>
        protected IEnumerable<SubscriptionMapping> Mappings => _mappings.Values.SelectMany(m => m);

        /// <summary>
        /// Adds a mapping for a topic's subscription defined within code to an actual host
        /// specific created subscription.
        /// </summary>
        /// <param name="mappings">The mapping settings.</param>
        public void AddMapping(params SubscriptionMapping[] mappings)
        {            
            if (mappings == null) throw new ArgumentNullException(nameof(mappings));

            foreach (var mapping in mappings)
            {
                if (! _mappings.TryGetValue(mapping.NamespaceName, out List<SubscriptionMapping> namespaceMappings))
                {
                    namespaceMappings = new List<SubscriptionMapping>();
                    _mappings.Add(mapping.NamespaceName, namespaceMappings);
                }
                
                AssertValidMapping(mapping.NamespaceName, namespaceMappings, mapping);
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
        public string GetMappedSubscription(SubscriptionMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
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