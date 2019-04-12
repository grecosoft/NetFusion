using System;
using Amqp;

namespace NetFusion.AMQP.Subscriber.Internal
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation specific logic for subscribing to a host defined queue.
    /// When a command is received on the corresponding queue, the associated event
    /// handler is invoked.   
    /// </summary>
    public class QueueSubscriberLinker : SubscriberLinkerBase,
        ISubscriberLinker
    {
        public void LinkSubscriber(Session session, HostItemSubscriber subscriber,
            ISubscriptionSettings subscriptionSettings)
        {
            var queueAttribute = (QueueAttribute)subscriber.HostItemAttribute;
            
            // Create a AMQP receiver link used to register handler method:
            var receiverLink = new ReceiverLink(session, 
                Guid.NewGuid().ToString(), 
                queueAttribute.QueueName);

            var logger = LoggerFactory.CreateLogger<QueueSubscriberLinker>();
            logger.LogInformation("Subscribing to queue {named}", queueAttribute.QueueName);
            
            ReceiveMessages(subscriber, receiverLink);
        }
    }
}