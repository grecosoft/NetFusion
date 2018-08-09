using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ;
using NetFusion.Messaging.Types;
using System.Threading;
using NetFusion.Messaging.Exceptions;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Strategy for publishing a RPC style message.
    /// </summary>
    internal class RpcPublisherStrategy : IPublisherStrategy
    {
        public async Task Publish(IPublisherContext context, CreatedExchange createdExchange, 
            IMessage message,
            CancellationToken cancellationToken)
        {
            ICommand command = (ICommand)message;
            string contentType = createdExchange.Meta.ContentType;

            byte[] messageBody = context.Serialization.Serialize(command, contentType);
            MessageProperties messageProperties = DefaultPublisherStrategy.GetMessageProperties(context, createdExchange, command);
            
            messageProperties.SetRpcActionNamespace(createdExchange.Meta.ActionNamespace);

            // Get the RPC client associated with the exchange to which the RPC message is being sent.
            IRpcClient client = context.PublisherModule.GetRpcClient(
                createdExchange.Meta.BusName,
                createdExchange.Meta.QueueMeta.QueueName);

            // Note:  Consumer replies with same content-type that was used to publish command.
            
            try
            {
                // Delegate to the client to send the request and wait for response in reply queue.
                byte[] resultBytes = await client.Publish(createdExchange, messageBody, 
                    messageProperties, 
                    cancellationToken);

                var responseObj = context.Serialization.Deserialize(contentType, command.ResultType, resultBytes);
                command.SetResult(responseObj);
                LogReceivedRpcResponse(context, createdExchange, responseObj);
            }
            catch (RpcReplyException ex)
            {
                if (ex.ReplayExceptionBody == null)
                {
                    throw;
                }

                var dispatchEx = context.Serialization.Deserialize<MessageDispatchException>(contentType, ex.ReplayExceptionBody);
                context.Logger.LogError(RabbitMqLogEvents.PublisherException, dispatchEx, "RPC Exception Reply.");
                throw dispatchEx;
            }
        }

        private static void LogReceivedRpcResponse(IPublisherContext context, CreatedExchange createdExchange, object responseObj)
        {
            var definition = createdExchange.Meta;
            
            context.Logger.LogTraceDetails(
                $"Response to RPC message sent to exchange: {definition.ExchangeName} on bus: {definition.BusName}", responseObj);
        }
    }
}