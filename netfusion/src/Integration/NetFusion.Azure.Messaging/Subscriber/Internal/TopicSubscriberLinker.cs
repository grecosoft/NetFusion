using System;
using Amqp;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Implementation specific logic for subscribing to a Namespace defined topic.
    /// When a domain-event is received on the corresponding topic's subscription,
    /// the associated event handler is invoked.   
    /// </summary>
    public class TopicSubscriberLinker : SubscriberLinkerBase,
        ISubscriberLinker
    {
        public void LinkSubscriber(Session session, NamespaceItemSubscriber subscriber)
        {
            var topicAttribute = (TopicAttribute)subscriber.NamespaceItemAttribute;
            
            // Create a AMQP receiver link used to register handler method:
            var receiverLink = new ReceiverLink(session, Guid.NewGuid().ToString(), 
                $"{topicAttribute.TopicName}/Subscriptions/{topicAttribute.SubscriptionName}");
            
            ReceiveMessages(subscriber, receiverLink);
        }
    }
}