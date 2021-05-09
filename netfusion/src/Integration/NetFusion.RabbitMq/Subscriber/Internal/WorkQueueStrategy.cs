using System;
using System.Threading.Tasks;
using EasyNetQ;
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
            var replyQueueIdentity = context.MessageProps.ReplyTo;
            if (replyQueueIdentity == null)
            {
                return;
            }

            try
            {
                await SendResponseToReplyQueue(context, response, replyQueueIdentity);
            }
            catch (Exception ex)
            {
                throw new MessageDispatchException(
                    $"Error sending command response to reply queue: {replyQueueIdentity}", ex);
            }
        }

        public static Task SendResponseToReplyQueue(ConsumeContext context, object response, string replyToQueue)
        {
            // Serialize the response message using the content-type of the original received message and set
            // the CorrelationId of the original message so the response can be matched with the sent command.
            var messageProps = new MessageProperties
            {
                ContentType = context.MessageProps.ContentType,
                CorrelationId = context.MessageProps.CorrelationId,
                MessageId = context.MessageProps.MessageId
            };

            return context.QueueResponse.RespondToSenderAsync(response, replyToQueue, messageProps);
        }
    }
}