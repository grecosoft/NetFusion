using Amqp;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Modules;

namespace NetFusion.AMQP.Subscriber.Internal
{
    /// <summary>
    /// Interface implemented to link a message handler method
    /// to a specific type of host item.
    /// </summary>
    public interface ISubscriberLinker
    {
        // Dependent Services:
        IMessageDispatchModule DispatchModule { get; }
        ISerializationManager Serialization { get; }
        ILoggerFactory LoggerFactory { get; }

        void SetServices(IMessageDispatchModule dispatchModule, ISerializationManager serialization,
            ILoggerFactory loggerFactory);
        
        /// <summary>
        /// Called to link a host item (i.e. Queue/Topic) to an event handler
        /// method that will be called when a message arrives on the item.
        /// </summary>
        /// <param name="session">The session to subscribe.</param>
        /// <param name="subscriber">Information about the host item and
        ///  handler method to be invoked.</param>
        /// <param name="subscriptionSettings">The registered subscription settings that
        /// determines how subscriptions are mapped to host created subscriptions.</param>
        void LinkSubscriber(Session session, HostItemSubscriber subscriber, 
            ISubscriptionSettings subscriptionSettings);
    }
}