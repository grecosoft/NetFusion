using IMessage = NetFusion.Messaging.Types.IMessage;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Messaging.Exceptions;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues for publishing RPC style messages and consuming their
    /// corresponding responds on an associated reply queue.
    /// </summary>
    internal class RpcQueueFactory: IQueueFactory
    {
        public void SetQueueDefaults(QueueDefinition definition)
        {
            definition.IsPassive = false;
            definition.IsDurable = false;
            definition.IsExclusive = false;
            definition.IsAutoDelete = true;

            // Append the unique Plugin Id associated with the hosting application
            // to the queue name.  This will make the name of the queue unique to
            // the application.
            definition.AppendHostId = true;
        }

        public void SetExchangeDefaults(QueueExchangeDefinition definition)
        {
            // A direct exchange will be used to receive RPC style messages where
            // the RouteKey is a value indicating the command.
            definition.ExchangeType = ExchangeType.Direct;
            definition.IsPassive = false;
            definition.IsDurable = false;
            definition.IsAutoDelete = true;
        }

        public IQueue CreateQueue(QueueContext context)
        {
            IQueue queue = context.CreateQueue();

            // Bind the queue to the exchange for each route-key.
            Debug.Assert(context.Definition.RouteKeys.Length == 1, 
                "RPC handler will only be assigned one route key used to identify the action to the consumer.");

            foreach (string routeKey in context.Definition.RouteKeys)
            {
                context.Bus.Advanced.Bind(context.Exchange, queue, routeKey);
            }

            return queue;
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