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
        public void LinkSubscriber(Session session, NamespaceItemSubscriber subscriber,
            ISubscriptionSettings subscriptionSettings)
        {
            var topicAttribute = (TopicAttribute)subscriber.NamespaceItemAttribute;
            
            // Determine if the host application has provided a specific subscription 
            // that should be used for the name specified in code.

            string namespaceName = subscriber.NamespaceAttribute.NamespaceName;
            var mapping = new SubscriptionMapping(topicAttribute.TopicName, topicAttribute.SubscriptionName);

            string subscriptionName = subscriptionSettings.GetMappedSubscription(namespaceName, mapping)
                                      ?? topicAttribute.SubscriptionName;
            
            // Create a AMQP receiver link used to register handler method:
            var receiverLink = new ReceiverLink(session, Guid.NewGuid().ToString(), 
                $"{topicAttribute.TopicName}/Subscriptions/{subscriptionName}");
            
            ReceiveMessages(subscriber, receiverLink);
        }
    }
}