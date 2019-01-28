using System;
using Amqp;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Implementation specific logic for subscribing to a Namespace defined queue.
    /// When a command is received on the corresponding queue, the associated event
    /// handler is invoked.   
    /// </summary>
    public class QueueSubscriberLinker : SubscriberLinkerBase,
        ISubscriberLinker
    {
        public void LinkSubscriber(Session session, NamespaceItemSubscriber subscriber,
            ISubscriptionSettings subscriptionSettings)
        {
            var topicAttribute = (QueueAttribute)subscriber.NamespaceItemAttribute;
            
            // Create a AMQP receiver link used to register handler method:
            var receiverLink = new ReceiverLink(session, 
                Guid.NewGuid().ToString(), 
                topicAttribute.QueueName);
            
            ReceiveMessages(subscriber, receiverLink);
        }
    }
}