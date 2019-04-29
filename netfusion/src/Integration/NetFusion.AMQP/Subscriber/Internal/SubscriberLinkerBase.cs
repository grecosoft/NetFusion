using System;
using Amqp;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using Amqp.Framing;
using NetFusion.Messaging.Plugin;

namespace NetFusion.AMQP.Subscriber.Internal
{
    /// <summary>
    /// Base class containing common code for binding a message to a type
    /// of host item.
    /// </summary>
    public abstract class SubscriberLinkerBase
    {
        // Dependent services set by caller:
        public IMessageDispatchModule DispatchModule { get; set; }
        public ISerializationManager Serialization { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }

        public void SetServices(IMessageDispatchModule dispatchModule, ISerializationManager serialization,
            ILoggerFactory loggerFactory)
        {
            DispatchModule = dispatchModule;
            Serialization = serialization;
            LoggerFactory = loggerFactory;
        }
        
        protected void ReceiveMessages(HostItemSubscriber subscriber, IReceiverLink receiverLink)
        {
            subscriber.SetReceiverLink(receiverLink);

            receiverLink.Closed += (sender, error) => { LogItemClosed(error); };
            
            // Start message pump that will be called when a message
            // is published to the host item.  
            receiverLink.Start(subscriber.HostItemAttribute.LinkCredit, (receiver, amqpMessage) => 
            {
                // Deserialize the message body into the the .NET types associated
                // with the handler to which the message should be dispatched.
                Type messageType = subscriber.DispatchInfo.MessageType;
                IMessage message = DeserializeMessage(amqpMessage, messageType);
                
                SetMessageApplicationProperties(amqpMessage, message);

                // Invoke the handler associated with the message.
                InvokeHandler(receiver, amqpMessage, message, subscriber);
            });
        }

        private static void SetMessageApplicationProperties(Message amqpMessage, IMessage message)
        {
            if (amqpMessage.ApplicationProperties?.Map == null) return;
            
            foreach (var item in amqpMessage.ApplicationProperties.Map)
            {
                message.Attributes.SetValue(item.Key.ToString(), item.Value);
            }
        }
        
        private void InvokeHandler(
            IReceiverLink receiverLink,
            Message amqpMessage,
            IMessage message, 
            HostItemSubscriber subscriber)
        {
            try
            {
                DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    subscriber.DispatchInfo, 
                    message).GetAwaiter().GetResult();   
                
                receiverLink.Accept(amqpMessage);
            }
            catch (Exception ex)
            {
                LogMessageReceiveEx(ex, message, subscriber.DispatchInfo);
                receiverLink.Reject(amqpMessage);
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
        
        private void LogItemClosed(Error error)
        {
            var logger = LoggerFactory.CreateLogger<SubscriberLinkerBase>();
            string errorDesc = error?.Description;
            
            if (errorDesc != null)
            {
                logger.LogDebug("ReceiverLink was closed.  Error: {error}", errorDesc);
            }
            else
            {
                logger.LogDebug("ReceiverLink was closed.");
            }
        }
    }
}