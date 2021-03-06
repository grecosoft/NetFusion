using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.RabbitMQ.Plugin;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Service implementation allowing replies, to prior sent messages, to be
    /// sent back to the sender with the response.
    /// </summary>
    public class QueueResponseService : IQueueResponseService
    {
        private const string MissingReplyPropMsg = "Replying to a command requires message to have property named: {0} specified.";
        
        private readonly IBusModule _busModule;
        private readonly ISerializationManager _serialization;

        public QueueResponseService(IBusModule busModule, ISerializationManager serialization)
        {
            _busModule = busModule ?? throw new ArgumentNullException(nameof(busModule));
            _serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
        }
        
        public Task RespondToSenderAsync(IMessage request, object response)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            MessageProperties props = GetReplyMessageProps(request);
            return RespondToSenderAsync(response, request.GetReplyTo(), props);
        }

        public Task RespondToSenderAsync(object response, string replyToQueue, MessageProperties messageProps)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            ValidateRequiredReplyProps(messageProps);
            
            byte[] messageBody = _serialization.Serialize(response, messageProps.ContentType);
            var (busName, queueName) = GetBusAndQueueName(replyToQueue);
            
            IBus bus = _busModule.GetBus(busName);
            
            return bus.Advanced.PublishAsync(Exchange.GetDefault(), queueName, false, 
                messageProps, 
                messageBody);
        }

        private static MessageProperties GetReplyMessageProps(IMessage message) => new()
            {
                ContentType = message.GetContentType(),
                CorrelationId = message.GetCorrelationId(),
                MessageId = message.GetMessageId()
            };

        private static (string, string) GetBusAndQueueName(string replyToQueue)
        {
            if (string.IsNullOrWhiteSpace(replyToQueue))
            {
                throw new InvalidOperationException(string.Format(MissingReplyPropMsg, "ReplyTo"));
            }
            
            string[] replyToQueueProps = replyToQueue.Split(":");
            if (replyToQueueProps.Length != 2)
            {
                throw new InvalidOperationException(
                    $"The ReplyTo message property of: {replyToQueue} does not specify " +
                    "the name of the message bus and queue joined by a : character.");
            }

            return (replyToQueueProps[0], replyToQueueProps[1]);
        }
        
        private static void ValidateRequiredReplyProps(MessageProperties messageProps)
        {
            if (!messageProps.CorrelationIdPresent)
                throw new InvalidOperationException(string.Format(MissingReplyPropMsg, "CorrelationId"));
            
            if (!messageProps.MessageIdPresent)
                throw new InvalidOperationException(string.Format(MissingReplyPropMsg, "MessageId"));
            
            if (!messageProps.ContentTypePresent)
                throw new InvalidOperationException(string.Format(MissingReplyPropMsg, "ContentType"));
        }
    }
}