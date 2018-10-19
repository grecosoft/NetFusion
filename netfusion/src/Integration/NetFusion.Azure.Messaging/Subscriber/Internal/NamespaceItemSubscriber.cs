using System;
using Amqp;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Core;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Class aggregating the information about a message handler
    /// and its assocated namespace item to which it should be bound.
    /// </summary>
    public class NamespaceItemSubscriber
    {
        public MessageDispatchInfo DispatchInfo { get; }
        public NamespaceAttribute NamespaceAttrib { get; }
        public NamespaceItemAttribute NamespaceItemAttrib { get; }
        public IReceiverLink ReceiverLink { get; private set; }
        
        public NamespaceItemSubscriber(MessageDispatchInfo dispatchInfo)
        {
            DispatchInfo = dispatchInfo ?? throw new ArgumentNullException(nameof(dispatchInfo));
            NamespaceAttrib = dispatchInfo.ConsumerType.GetAttribute<NamespaceAttribute>();
            NamespaceItemAttrib = dispatchInfo.MessageHandlerMethod.GetAttribute<NamespaceItemAttribute>();
        }
        
        // Any dispatch corresponding to a method decorated with a derived
        // AzureNamespace attribute will be bound as a receiver.
        public static bool IsSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null)
                throw new ArgumentNullException(nameof(dispatchInfo));

            return
                dispatchInfo.MessageHandlerMethod.HasAttribute<NamespaceItemAttribute>()
                && dispatchInfo.ConsumerType.HasAttribute<NamespaceAttribute>();
        }

        public void SetReceiverLink(IReceiverLink link)
        {
            ReceiverLink = link ?? throw new ArgumentNullException(nameof(link));
        }
    }
}