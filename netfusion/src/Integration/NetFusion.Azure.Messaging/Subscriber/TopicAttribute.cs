using System;
using NetFusion.Azure.Messaging.Subscriber.Internal;

namespace NetFusion.Azure.Messaging.Subscriber
{
    /// <summary>
    /// Attribute used to specify a topic defined on a namespace that
    /// should be invoked when a message arrives matching the criteria
    /// of the specified subscription.
    /// </summary>
    public class TopicAttribute : NamespaceItemAttribute
    {
        /// <summary>
        /// The name of the configured topic.
        /// </summary>
        public string TopicName { get; }
        
        /// <summary>
        /// The name of the configured subscription.
        /// </summary>
        public string SubscriptionName { get; set; }

        public TopicAttribute(string topic, string subscription)
            : base (new TopicSubscriberLinker())
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic name must be specified", nameof(topic));
            
            if (string.IsNullOrWhiteSpace(subscription))
                throw new ArgumentException("Subscription name must be specified", nameof(subscription));

            TopicName = topic;
            SubscriptionName = subscription;
        }
    }
}