using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Logging;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating work queues on the default exchange.  Sets the default
    /// conventions used for a work queues queues.</summary>
    internal class WorkQueueStrategy : IQueueStrategy
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.DefineDefault(attribute.BusName, attribute.QueueName,
                meta =>
                {
                    meta.IsAutoDelete = false;
                    meta.IsDurable = true;
                    meta.IsExclusive = false;
                    meta.IsUnacknowledgedSaved = attribute.IsUnacknowledgedSaved;
                });

            return exchange.QueueMeta;
        }

        public async Task OnMessageReceivedAsync(ConsumeContext context)
        {
            var logger = context.LoggerFactory.CreateLogger<WorkQueueStrategy>();
            var message = context.DeserializeIntoMessage();
            
            var msgLog = new MessageLog(message, LogContextType.ReceivedMessage);
            msgLog.SentHint("subscribe-rabbitmq");
            
            context.LogReceivedMessage(message);
            context.AddMessageContextToLog(msgLog);
            
            try
            {
                var response = await context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    context.Subscriber.DispatchInfo, 
                    message);

                if (response != null)
                {
                    await RespondToReplyQueue(logger, context, response);
                }
            }
            catch (Exception ex)
            {
                msgLog.AddLogError("Queue Subscription", ex);
                throw;
            }
            finally { await context.MessageLogger.LogAsync(msgLog); }
        }

        private static async Task RespondToReplyQueue(ILogger logger, ConsumeContext context, object response)
        {
            // Determine of the publisher expects a response to the command
            var replyToValue = context.MessageProps.ReplyTo;
            if (replyToValue == null)
            {
                return;
            }
            
            string[] replyToQueueProps = replyToValue.Split(":");
            if (replyToQueueProps.Length != 2)
            {
                logger.LogError($"The ReplyTo message property of: {replyToValue} does not specify " 
                                + "the name of the message bus and queue joined by a : character.");
                return;
            }

            try
            {
                await SendResponseToReplyQueue(context, response, replyToQueueProps[0], replyToQueueProps[1]);
            }
            catch (Exception ex)
            {
                throw new MessageDispatchException(
                    $"Error sending command response to reply queue: {replyToValue}", ex);
            }
        }

        public static async Task SendResponseToReplyQueue(ConsumeContext context, object response, string busName, string queueName)
        {
            // Serialize the response message using the content-type of the original received message and set
            // the CorrelationId of the original message so the response can be matched with the sent command.
            byte[] messageBody = context.Serialization.Serialize(response, context.MessageProps.ContentType);
            var messageProps = new MessageProperties
            {
                ContentType = context.MessageProps.ContentType,
                CorrelationId = context.MessageProps.CorrelationId
            };
                
            IBus bus = context.BusModule.GetBus(busName);
            await bus.Advanced.PublishAsync(Exchange.GetDefault(), queueName, false, messageProps, messageBody);
        }
    }
}