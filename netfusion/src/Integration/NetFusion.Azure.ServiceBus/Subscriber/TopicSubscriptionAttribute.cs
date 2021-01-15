using System;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Attribute used to specific a message handler method that should be bound
    /// to a Topic Subscription can called when a Domain Event is received.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class TopicSubscriptionAttribute: SubscriptionAttribute
    {
        public string SubscriptionName { get; }
        
        /// <summary>
        /// Indicates that messages should be delivered to all consumers and not round-robin.  If set, an
        /// unique quid is append to the subscription name making it unique per running Microservice instance.
        /// </summary>
        public bool IsFanout { get; set; }
        
        public TopicSubscriptionAttribute(string namespaceName, string topicName, string subscriptionName)
            : base(namespaceName, topicName)
        {
            SubscriptionName = subscriptionName;
        }
    }
}