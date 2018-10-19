using System;
using Amqp;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Implemention specific logic for subscribing to a Namespace defined topic.
    /// When a domain-event is received on the corresponding topic's subscription,
    /// the associated event handler is invoked.   
    /// </summary>
    public class TopicSubscriberLinker : SubscriberLinkerBase,
        ISubscriberLinker
    {
        public void LinkSubscriber(Session session, NamespaceItemSubscriber subscriber)
        {
            var topicAttrib = (TopicAttribute)subscriber.NamespaceItemAttrib;
            
            // Create a AMQP receiver link used to register handler method:
            var receiverLink = new ReceiverLink(session, Guid.NewGuid().ToString(), 
                $"{topicAttrib.TopicName}/Subscriptions/{topicAttrib.SubscriptionName}");
            
            ReceiveMessages(subscriber, receiverLink);
        }
    }
}