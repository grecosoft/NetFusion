using System;
using Amqp;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Internal;

namespace NetFusion.AMQP.Subscriber.Internal
{
    /// <summary>
    /// Class aggregating the information about a message handler
    /// and its associated host item to which it should be bound.
    /// </summary>
    public class HostItemSubscriber
    {
        public MessageDispatchInfo DispatchInfo { get; }
        public HostAttribute HostAttribute { get; }
        public HostItemAttribute HostItemAttribute { get; }
        
        // AMQP associated objects:
        public IReceiverLink ReceiverLink { get; private set; }
        
        public HostItemSubscriber(MessageDispatchInfo dispatchInfo)
        {
            DispatchInfo = dispatchInfo ?? throw new ArgumentNullException(nameof(dispatchInfo));
            HostAttribute = dispatchInfo.ConsumerType.GetAttribute<HostAttribute>();
            HostItemAttribute = dispatchInfo.MessageHandlerMethod.GetAttribute<HostItemAttribute>();
        }
        
        // Any dispatch corresponding to a method decorated with a derived
        // HostItem attribute will be bound as a receiver.
        public static bool IsSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null)
                throw new ArgumentNullException(nameof(dispatchInfo));

            return
                dispatchInfo.MessageHandlerMethod.HasAttribute<HostItemAttribute>()
                && dispatchInfo.ConsumerType.HasAttribute<HostAttribute>();
        }

        public void SetReceiverLink(IReceiverLink link)
        {
            ReceiverLink = link ?? throw new ArgumentNullException(nameof(link));
        }
    }
}