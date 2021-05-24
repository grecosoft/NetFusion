using System;

namespace NetFusion.Azure.ServiceBus.Subscriber.Internal
{
    /// <summary>
    /// Base class derived by specific namespace entity subscription attributes.
    /// </summary>
    public abstract class SubscriptionAttribute : Attribute
    {
        /// <summary>
        /// The namespace containing the entity to subscribe.
        /// </summary>
        public string NamespaceName { get; }
        
        /// <summary>
        /// The name of the entity within the namespace to subscribe.
        /// </summary>
        public string EntityName { get; }

        protected SubscriptionAttribute(string namespaceName, string entityName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace Name must be specified.", nameof(namespaceName));
            
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("Entity Name must be specified.", nameof(entityName));
            
            NamespaceName = namespaceName;
            EntityName = entityName;
        }
    }
}