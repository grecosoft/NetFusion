using System;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Attribute used to specific a message handler method bound to a Topic 
    /// Subscription and called when a Domain Event is received.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class TopicSubscriptionAttribute: SubscriptionAttribute
    {
        /// <summary>
        /// The name of the subscription, bound to the topic, to which messages are delivered.
        /// </summary>
        public string SubscriptionName { get; }
        
        /// <summary>
        /// Indicates that messages should be delivered to all consumers and not round-robin.  If set, an
        /// unique quid is append to the subscription name making it unique per running Microservice instance.
        /// </summary>
        public bool IsFanout { get; set; }
        
        /// <summary>
        /// Binds domain-event handler to a specific subscription bound to a topic within 
        /// an Azure Service Bus namespace.
        /// </summary>
        /// <param name="namespaceName">The defined namespace containing the topic.</param>
        /// <param name="topicName">The name of the topic defined within the namespace.</param>
        /// <param name="subscriptionName">The name of the subscription, bound to the topic, to which 
        /// messages are delivered.</param>
        public TopicSubscriptionAttribute(string namespaceName, string topicName, string subscriptionName)
            : base(namespaceName, topicName)
        {
            SubscriptionName = subscriptionName;
        }
    }
}