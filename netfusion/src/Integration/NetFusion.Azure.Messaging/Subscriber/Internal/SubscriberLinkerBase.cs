using System;
using Amqp;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Base class containing common code for binding a message to a type
    /// of namespace item.
    /// </summary>
    public abstract class SubscriberLinkerBase
    {
        // Dependent services set by caller:
        public IMessageDispatchModule DispatchModule { get; set; }
        public ISerializationManager Serialization { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        
        protected void ReceiveMessages(NamespaceItemSubscriber subscriber, IReceiverLink receiverLink)
        {
            subscriber.SetReceiverLink(receiverLink);
            
            // Start message pump that will be called when a message
            // is published to the topic.  
            receiverLink.Start(subscriber.NamespaceItemAttribute.LinkCredit, (receiver, nsMessage) => 
            {
                // Deserialize the message body into the the .NET types associated
                // with the handler to which the message should be dispatched.
                Type messageType = subscriber.DispatchInfo.MessageType;
                IMessage message = DeserializeMessage(nsMessage, messageType);

                // Invoke the handler associated with the message.
                InvokeHandler(receiver, nsMessage, message, subscriber);
            });
        }
        
        private void InvokeHandler(
            IReceiverLink receiverLink,
            Message nsMessage,
            IMessage message, 
            NamespaceItemSubscriber subscriber)
        {
            try
            {
                DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    subscriber.DispatchInfo, 
                    message).GetAwaiter().GetResult();   
                
                receiverLink.Accept(nsMessage);
            }
            catch (Exception ex)
            {
                LogMessageReceiveEx(ex, message, subscriber.DispatchInfo);
                receiverLink.Reject(nsMessage);
            }
        }

        private void LogMessageReceiveEx(Exception ex, IMessage message, MessageDispatchInfo dispatchInfo)
        {
            var logger = LoggerFactory.CreateLogger<SubscriberLinkerBase>();
            
            logger.LogError(ex, 
                "Error handling received message of type: {MessageType} when calling " + 
                "the consumer: {Consumer} with handler: {Handler}.", 
                message.GetType(), 
                dispatchInfo.ConsumerType, 
                dispatchInfo.MessageHandlerMethod.Name);
        }
        
        private IMessage DeserializeMessage(Message nsMessage, Type messageType)
        {
            string contentType = nsMessage.Properties.ContentType;
            return (IMessage)Serialization.Deserialize(contentType, messageType, (byte[])nsMessage.Body);
        }
    }
}