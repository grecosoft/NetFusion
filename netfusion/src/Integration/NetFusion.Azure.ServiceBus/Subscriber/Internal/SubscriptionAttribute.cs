using System;

namespace NetFusion.Azure.ServiceBus.Subscriber.Internal
{
    /// <summary>
    /// Base class derived by specific namespace entity subscription attributes.
    /// </summary>
    public abstract class SubscriptionAttribute : Attribute
    {
        public string NamespaceName { get; }
        public string EntityName { get; }

        protected SubscriptionAttribute(string namespaceName, string entityName)
        {
            NamespaceName = namespaceName;
            EntityName = entityName;
        }
    }
}