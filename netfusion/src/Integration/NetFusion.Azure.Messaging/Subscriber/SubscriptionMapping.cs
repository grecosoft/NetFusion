namespace NetFusion.Azure.Messaging.Subscriber
{
    using System;

    public class SubscriptionMapping
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public string MappedSubscriptionName { get; set; }

        public SubscriptionMapping(string topicName, string subscriptionName, 
            string mappedSubscriptionName) : this(topicName, subscriptionName)
        {  
            if (string.IsNullOrWhiteSpace(mappedSubscriptionName))
                throw new ArgumentException("Mapped subscription name not specified.", nameof(mappedSubscriptionName));

            MappedSubscriptionName = mappedSubscriptionName;
        }
        
        internal SubscriptionMapping(string topicName, string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(topicName))    
                throw new ArgumentException("Topic name not specified.", nameof(topicName));
            
            if (string.IsNullOrWhiteSpace(subscriptionName))
                
                throw new ArgumentException("Subscription name not specified.", nameof(subscriptionName));

            TopicName = topicName;
            SubscriptionName = subscriptionName;
        }
    }
}