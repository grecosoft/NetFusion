using Amqp;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Modules;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Interface implemented to link a message handler method
    /// to a specific type of namespace item.
    /// </summary>
    public interface ISubscriberLinker
    {
        // Dependent Services:
        IMessageDispatchModule DispatchModule { get; set; }
        ISerializationManager Serialization { get; set; }
        ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// Called to link a namespace item (i.e. Queue/Topic) to an event handler
        /// method that will be called when a message arrives on the namespace item.
        /// </summary>
        /// <param name="session">The session to subscribe.</param>
        /// <param name="subscriber">Information about the namespace item and
        /// and the handler method to be invoked.</param>
        /// <param name="subscriptionSettings">The registered subscription settings that
        /// determines how subscriptions are mapped to host created subscriptions.</param>
        void LinkSubscriber(Session session, NamespaceItemSubscriber subscriber, 
            ISubscriptionSettings subscriptionSettings);
    }
}