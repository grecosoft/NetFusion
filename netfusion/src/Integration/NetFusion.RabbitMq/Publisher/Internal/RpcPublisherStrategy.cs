using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;
using System.Threading.Tasks;
using EasyNetQ;
using System.Threading;
using NetFusion.Messaging.Exceptions;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
using NetFusion.Messaging.Types.Contracts;

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
            string contentType = createdExchange.Definition.ContentType;

            // Serialize the message and get the properties from the default-publisher to be used as 
            // the initial list of message properties to which RPC specific properties will be added.
            byte[] messageBody = context.Serialization.Serialize(command, contentType);
            MessageProperties messageProperties = DefaultPublisherStrategy.GetMessageProperties(context, createdExchange, command);
            
            // Get the RPC client associated with the exchange to which the RPC message is being sent.
            IRpcClient client = context.PublisherModule.GetRpcClient(
                createdExchange.Definition.BusName,
                createdExchange.Definition.QueueMeta.QueueName);

            // Note:  Consumer replies with same content-type that was used to publish command.
            
            try
            {
                // Delegate to the client to send the request and wait for response in reply queue.
                byte[] resultBytes = await client.Publish(createdExchange, messageBody, 
                    messageProperties, 
                    cancellationToken).ConfigureAwait(false);

                // If a successful reply, deserialize the response message into the
                // result type associated with the command.
                var responseObj = context.Serialization.Deserialize(contentType, command.ResultType, resultBytes);
                command.SetResult(responseObj);

                LogReceivedRpcResponse(context, createdExchange, responseObj);
            }
            catch (RpcReplyException ex)
            {
                // If the consumer didn't supply details about the exception, then just rethrow.
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
            var definition = createdExchange.Definition;
            
            context.Logger.LogTraceDetails(
                $"Response to RPC message sent to queue: {definition.QueueMeta.QueueName} on bus: {definition.BusName}", responseObj);
        }
    }
}