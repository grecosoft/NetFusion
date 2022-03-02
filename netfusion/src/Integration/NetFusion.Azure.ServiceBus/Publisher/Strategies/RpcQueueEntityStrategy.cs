using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using NetFusion.Messaging.Types.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Strategy for publishing a RPC style command and recording a future task
    /// completed when the corresponding result is received on the reply queue.
    /// The message's MessageId value is used to correlate the response to the
    /// original sent command.
    /// </summary>
    internal class RpcQueueEntityStrategy : NoOpEntityStrategy,
        IPublishStrategy,
        IRequiresContext
    {
        private readonly RpcQueueSourceMeta _rpcQueueMeta;

        public RpcQueueEntityStrategy(RpcQueueSourceMeta rpcQueueMeta)
        {
            _rpcQueueMeta = rpcQueueMeta;
        }

        public NamespaceContext Context { get; set; }


        // ------------------ RPC Command Dispatching -------------------

        public async Task SendToEntityAsync(IMessage message, ServiceBusMessage busMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                // Since the publisher of a RPC message will timeout after a specified number of milliseconds,
                // the message's TimeToLive will be set to the same value.  This way the message will not remain
                // within the queue, since after the timeout, the publisher will have deleted the record of sending
                // the message.
                busMessage.TimeToLive = TimeSpan.FromMilliseconds(_rpcQueueMeta.CancelRpcRequestAfterMs);

                var futureResult = RecordSentMessage(busMessage, cancellationToken);
                await _rpcQueueMeta.EntitySender.SendMessageAsync(busMessage, cancellationToken);

                byte[] resultBytes = await futureResult.Task;
                SetCommandResult(message, resultBytes);

            }
            catch (TaskCanceledException ex)
            {
                if (_rpcQueueMeta.ReplyQueue.PendingRpcRequests.TryRemove(busMessage.MessageId,
                    out RpcPendingRequest pendingRequest))
                {
                    pendingRequest.UnRegister();
                }

                throw new RpcReplyException(
                    $"The RPC request with the Message Id of: {busMessage.MessageId} was canceled. " +
                    $"The current timeout value is: {_rpcQueueMeta.CancelRpcRequestAfterMs} ms.",
                    ex);
            }
            catch (Exception ex)
            {
                if (_rpcQueueMeta.ReplyQueue.PendingRpcRequests.TryRemove(busMessage.MessageId,
                    out RpcPendingRequest pendingRequest))
                {
                    pendingRequest.UnRegister();
                }
                throw new RpcReplyException(
                    $"The RPC request with the Message Id of: {busMessage.MessageId} resulted in an exception.", ex);
            }
        }

        // Returns a task source associated with the Message Id of the sent RPC command.
        // When a response is received on the reply queue, the task source is either marked
        // as completed or with an error.  
        private TaskCompletionSource<byte[]> RecordSentMessage(ServiceBusMessage busMessage,
            CancellationToken cancellationToken)
        {
            int cancelRpcRequestAfterMs = _rpcQueueMeta.CancelRpcRequestAfterMs;

            // Create a task that can be completed in the future when the result
            // is received in the reply queue. 
            var futureResult = new TaskCompletionSource<byte[]>();
            var rpcPendingRequest = new RpcPendingRequest(futureResult, cancelRpcRequestAfterMs, cancellationToken);

            _rpcQueueMeta.ReplyQueue.PendingRpcRequests[busMessage.MessageId] = rpcPendingRequest;
            return futureResult;
        }

        private void SetCommandResult(IMessage message, byte[] resultBytes)
        {
            // If a successful reply, deserialize the response message into the
            // result type associated with the command.
            var commandResult = (ICommandResultState)message;

            var responseObj = Context.Serialization.Deserialize(_rpcQueueMeta.ContentType,
                commandResult.DeclaredResultType,
                resultBytes);

            commandResult.SetResult(responseObj);
        }
    }
}
