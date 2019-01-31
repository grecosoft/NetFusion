using System;
using Amqp;

namespace NetFusion.AMQP.Subscriber.Internal
{
    /// <summary>
    /// Implementation specific logic for subscribing to a host defined topic.
    /// When a domain-event is received on the corresponding topic's subscription,
    /// the associated event handler is invoked.   
    /// </summary>
    public class TopicSubscriberLinker : SubscriberLinkerBase,
        ISubscriberLinker
    {
        public void LinkSubscriber(Session session, HostItemSubscriber subscriber,
            ISubscriptionSettings subscriptionSettings)
        {
            var topicAttribute = (TopicAttribute)subscriber.HostItemAttribute;
            
            // Determine if the host application has provided a specific subscription 
            // that should be used for the name specified in code.

            string hostName = subscriber.HostAttribute.HostName;
            
            var mapping = new SubscriptionMapping(hostName, topicAttribute.TopicName, 
                topicAttribute.SubscriptionName);

            string subscriptionName = subscriptionSettings.GetMappedSubscription(mapping)
                                      ?? topicAttribute.SubscriptionName;
            
            // Create a AMQP receiver link used to register handler method:
            var receiverLink = new ReceiverLink(session, Guid.NewGuid().ToString(), 
                $"{topicAttribute.TopicName}/Subscriptions/{subscriptionName}");
            
            ReceiveMessages(subscriber, receiverLink);
        }
    }
}