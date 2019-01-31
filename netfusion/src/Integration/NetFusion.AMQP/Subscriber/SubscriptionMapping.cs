using System;

namespace NetFusion.AMQP.Subscriber
{
    public class SubscriptionMapping
    {
        public string HostName { get; set; }
        public string HostItemName { get; set; }
        public string SubscriptionName { get; set; }
        public string MappedSubscriptionName { get; set; }

        public SubscriptionMapping(
            string hostName,
            string hostItemName, 
            string subscriptionName, 
            string mappedSubscriptionName) : this(hostName, hostItemName, subscriptionName)
        {  
            if (string.IsNullOrWhiteSpace(mappedSubscriptionName))
                throw new ArgumentException("Mapped subscription name not specified.", nameof(mappedSubscriptionName));

            MappedSubscriptionName = mappedSubscriptionName;
        }
        
        internal SubscriptionMapping(
            string hostName,
            string hostItemName, 
            string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(hostName))    
                throw new ArgumentException("Host name not specified.", nameof(hostName));
            
            if (string.IsNullOrWhiteSpace(hostItemName))    
                throw new ArgumentException("Host Item name not specified.", nameof(hostItemName));
            
            if (string.IsNullOrWhiteSpace(subscriptionName))
                
                throw new ArgumentException("Subscription name not specified.", nameof(subscriptionName));

            HostName = hostName;
            HostItemName = hostItemName;
            SubscriptionName = subscriptionName;
        }
    }
}