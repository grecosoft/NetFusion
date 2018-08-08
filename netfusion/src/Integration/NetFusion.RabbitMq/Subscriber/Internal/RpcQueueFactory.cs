using IMessage = NetFusion.Messaging.Types.IMessage;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Messaging.Exceptions;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues for publishing RPC style messages and consuming their
    /// corresponding responds on an associated reply queue.
    /// </summary>
    internal class RpcQueueFactory: IQueueFactory
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var rpcAttrib = (RpcQueueAttribute)attribute;
            
            var exchange = ExchangeMeta.DefineDefault(rpcAttrib.BusName, rpcAttrib.QueueName,
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsPassive = false;
                    config.IsExclusive = false;
                });

            exchange.ActionName = rpcAttrib.ActionName;
            return exchange.QueueMeta;
        }
        
        public async Task OnMessageReceived(ConsumeContext context, IMessage message)
        {
            try 
            {
                object response = await context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    context.Subscriber.DispatchInfo, 
                    message);

                await ReplyWithResponse(context, response);
            }
            catch (AggregateException ex)
            {
                await ReplyWithException(context, ex.InnerException);
            }
            catch (Exception ex)
            {
                await ReplyWithException(context, ex);
            }
        }

        private static Task ReplyWithResponse(ConsumeContext context, object response)
        {
            var replyMessageProps = GetReplyMessageProps(context);
            return PublishConsumerReply(context, replyMessageProps, response);
        }

        private static Task ReplyWithException(ConsumeContext context, Exception ex)
        {
            if ( !(ex is MessageDispatchException dispatchEx))
            {
                dispatchEx = new MessageDispatchException(ex.Message);
            }

            var replyMessageProps = GetReplyMessageProps(context);
            replyMessageProps.SetRpcReplyException(true);

            return PublishConsumerReply(context, replyMessageProps, dispatchEx);
        }

        private static async Task PublishConsumerReply(ConsumeContext context, 
            MessageProperties replyMessageProps, 
            object response)
        {
            string busReplyToKey = context.MessageProps.GetRpcReplyBusConfigName();
            string replyToQueue = GetReplyToQueue(context.MessageProps);

            IBus replyToBus = context.BusModule.GetBus(busReplyToKey);
            byte[] replyBody = context.Serialization.Serialize(response, replyMessageProps.ContentType);

            await replyToBus.Advanced.PublishAsync(Exchange.GetDefault(), replyToQueue, 
                false,
                replyMessageProps,
                replyBody);
        }

        // Reply with the same content type as that of the received command.
        private static MessageProperties GetReplyMessageProps(ConsumeContext context) =>
            new MessageProperties {
                ContentType = context.MessageProps.ContentType,
                CorrelationId = context.MessageProps.CorrelationId
            };

        private static string GetReplyToQueue(MessageProperties messageProps)
        {
            if (string.IsNullOrWhiteSpace(messageProps.ReplyTo))
            {
                throw new InvalidOperationException("ReplyTo message property not set.");
            }   
            
            return messageProps.ReplyTo;
        }
    }
}