using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;
using System;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Plugin;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Class aggregating a set of references associated with
    /// the context of a received message.
    /// </summary>
    public class ConsumeContext 
    {
        public ILoggerFactory LoggerFactory { get; set; }
        
        public byte[] MessageData { get; set; }
        
        // Metadata and received message properties:
        public MessageProperties MessageProps { get; internal set; }
        public MessageReceivedInfo MessageReceiveInfo { get; internal set; }
        public MessageQueueSubscriber Subscriber { get; internal set; }

        // Services:
        public IBusModule BusModule { get; internal set; }
        public IMessageDispatchModule MessagingModule { get; internal set; }
        public ISerializationManager Serialization { get; internal set; }
        public IMessageLogger MessageLogger { get; internal set; }

        public Func<string, string, MessageDispatchInfo> GetRpcMessageHandler { get; internal set; }
        
        /// <summary>
        /// Returns the received message data as a deserialized message using the
        /// information recorded within the context.
        /// </summary>
        /// <returns>Reference to deserialized object.</returns>
        public IMessage DeserializeIntoMessage()
        {
            Type messageType = Subscriber.DispatchInfo.MessageType;
            object message = Serialization.Deserialize(MessageProps.ContentType, messageType, MessageData);
            return (IMessage)message;
        }
        
        /// <summary>
        /// Returns the received message data as a deserialized message using the
        /// passed message type and the information recorded within the context.
        /// </summary>
        /// <param name="messageType">The type of the message to deserialize message data into.</param>
        /// <returns>Reference to deserialized object.</returns>
        public IMessage DeserializeIntoMessage(Type messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            
            object message = Serialization.Deserialize(MessageProps.ContentType, messageType, MessageData);
            return (IMessage)message;
        }
        // ---------------------------------- [Logging] ----------------------------------
        
        /// <summary>
        /// Logs the provided deserialized message and the information about the exchange and
        /// queue from which it was received.
        /// </summary>
        /// <param name="message">The received message.</param>
        public void LogReceivedMessage(IMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var logger = LoggerFactory.CreateLogger<ConsumeContext>();

            var queueInfo = new {
                Bus = Subscriber.QueueMeta.Exchange.BusName,
                Exchange = Subscriber.QueueMeta.Exchange.ExchangeName,
                Queue = Subscriber.QueueMeta.QueueName,
                MessageProps.ContentType,
                Consumer = Subscriber.DispatchInfo.ConsumerType.Name,
                Handler = Subscriber.DispatchInfo.MessageHandlerMethod.Name
            };

            var log = LogMessage.For(LogLevel.Information, "Message {MessageType} Received from {Queue} on {Bus}",
                message.GetType(),
                queueInfo.Queue,
                queueInfo.Bus).WithProperties(
                    new LogProperty { Name = "Message", Value = message },
                    new LogProperty { Name = "QueueInfo", Value = queueInfo });
            
            logger.Log(log);
        }

        public void AddMessageContextToLog(MessageLog msgLog)
        {
            if (msgLog == null) throw new ArgumentNullException(nameof(msgLog));
            
            msgLog.AddLogDetail("Exchange Name", Subscriber.QueueMeta.Exchange.ExchangeName);
            msgLog.AddLogDetail("Exchange Type", Subscriber.QueueMeta.Exchange.ExchangeType);
            msgLog.AddLogDetail("Queue Name", Subscriber.QueueMeta.QueueName);
            msgLog.AddLogDetail("Content Type", MessageProps.ContentType);
            msgLog.AddLogDetail("Handler Class", Subscriber.DispatchInfo.ConsumerType.Name);
            msgLog.AddLogDetail("Handler Method", Subscriber.DispatchInfo.MessageHandlerMethod.Name);
        }
    }
}