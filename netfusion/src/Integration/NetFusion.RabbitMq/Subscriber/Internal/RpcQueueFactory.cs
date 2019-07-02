using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Internal;
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
            var rpcAttribute = (RpcQueueAttribute)attribute;
            
            var exchange = ExchangeMeta.DefineDefault(rpcAttribute.BusName, rpcAttribute.QueueName,
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsPassive = false;
                    config.IsExclusive = false;
                });

            exchange.IsRpcExchange = true;
            exchange.ActionNamespace = rpcAttribute.ActionNamespace;           
            return exchange.QueueMeta;
        }
        
        // When a RPC style command message is received, it is dispatched to the in-process handler having the
        // matching queue name and action-namespace.  This is unlike the other message patterns where a queue 
        // is associated directly with only a single handler.  This allows for several RPC style commands to
        // use the same queue.  This allows for more efficient use of queues.
        public async Task OnMessageReceivedAsync(ConsumeContext context)
        {
            MessageDispatchInfo rpcCommandHandler = GetDispatchInfoForRpcCommand(context);
            var message = context.DeserializeIntoMessage(rpcCommandHandler.MessageType);
            
            context.LogReceivedMessage(message);
           
            try 
            {
                object response = await context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    rpcCommandHandler, 
                    message).ConfigureAwait(false);

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

        private static MessageDispatchInfo GetDispatchInfoForRpcCommand(ConsumeContext context)
        {
            string rpcQueueName = context.Subscriber.QueueMeta.QueueName;
            string rpcActionNamespace = context.MessageProps.GetRpcActionNamespace();
            
            context.Logger.LogTrace(
                "RPC command received.  Attempting to dispatch to handler associated with queue named {queueName} and " + 
                "for action named {actionName}", rpcQueueName, rpcActionNamespace);

            return context.GetRpcMessageHandler(rpcQueueName, rpcActionNamespace);
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