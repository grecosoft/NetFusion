using System;

namespace NetFusion.Azure.Messaging.Subscriber
{
    public class SubscriptionMapping
    {
        public string NamespaceName { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public string MappedSubscriptionName { get; set; }

        public SubscriptionMapping(
            string namespaceName,
            string topicName, 
            string subscriptionName, 
            string mappedSubscriptionName) : this(namespaceName, topicName, subscriptionName)
        {  
            if (string.IsNullOrWhiteSpace(mappedSubscriptionName))
                throw new ArgumentException("Mapped subscription name not specified.", nameof(mappedSubscriptionName));

            MappedSubscriptionName = mappedSubscriptionName;
        }
        
        internal SubscriptionMapping(
            string namespaceName,
            string topicName, 
            string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))    
                throw new ArgumentException("Namespace name not specified.", nameof(topicName));
            
            if (string.IsNullOrWhiteSpace(topicName))    
                throw new ArgumentException("Topic name not specified.", nameof(topicName));
            
            if (string.IsNullOrWhiteSpace(subscriptionName))
                
                throw new ArgumentException("Subscription name not specified.", nameof(subscriptionName));

            NamespaceName = namespaceName;
            TopicName = topicName;
            SubscriptionName = subscriptionName;
        }
    }
}