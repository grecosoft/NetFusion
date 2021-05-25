using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Custom publish strategy containing logic coordinating the publishing
    /// of commands and processing the corresponding reply response.
    /// </summary>
    public class RpcQueueStrategy : IEntityStrategy,
        IRequiresContext,
        IPublishStrategy,
        ICleanupStrategy
    {
        private readonly RpcQueueSourceMeta _rpcQueueMeta;
        
        // Dictionary containing the pending task associated with the outgoing
        // RPC command keyed by correlation id.  
        private readonly ConcurrentDictionary<string, RpcPendingRequest> _pendingRpcRequests;

        // Process RPC reply messages and updates state of RpcPendingRequest. 
        private ServiceBusProcessor _replyProcessor;
        
        public RpcQueueStrategy(RpcQueueSourceMeta queueMeta)
        {
            _pendingRpcRequests = new ConcurrentDictionary<string, RpcPendingRequest>();
            _rpcQueueMeta = queueMeta;
        }
        
        public NamespaceContext Context { get; set; }
        
        
        // ------------------ RPC Command Dispatching -------------------
        
        public async Task SendToEntityAsync(IMessage message, ServiceBusMessage busMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                var futureResult = RecordSentMessage(busMessage, cancellationToken);
                await _rpcQueueMeta.EntitySender.SendMessageAsync(busMessage, cancellationToken); 
                
                byte[] resultBytes = await futureResult.Task;
                SetCommandResult(message, resultBytes);
                
            }
            catch (TaskCanceledException ex)
            {
                // If the pending request task has been canceled, remove the pending
                // request and unregister the cancellation delegate.
                if (_pendingRpcRequests.TryRemove(busMessage.CorrelationId, out RpcPendingRequest pendingRequest))
                {
                    pendingRequest.UnRegister();
                }
                
                throw new RpcReplyException(
                    $"The RPC request with the correlation value of: {busMessage.CorrelationId} was canceled. " + 
                    $"The current timeout value is: {_rpcQueueMeta.CancelRpcRequestAfterMs} ms.", 
                    ex);
            }
            catch (Exception ex)
            {
                if (_pendingRpcRequests.TryRemove(busMessage.CorrelationId, out RpcPendingRequest pendingRequest))
                {
                    pendingRequest.UnRegister();
                }
                throw new RpcReplyException(
                    $"The RPC request with the correlation value of: {busMessage.CorrelationId} resulted in an exception.", ex);
            }
        }
        
        // Returns a task source associated with the Correlation Id of the sent RPC command.
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
            
            _pendingRpcRequests[busMessage.CorrelationId] = rpcPendingRequest;
            return futureResult;
        }
        
        private void SetCommandResult(IMessage message, byte[] resultBytes)
        {
            // If a successful reply, deserialize the response message into the
            // result type associated with the command.
            var commandResult = (ICommandResultState)message;
            
            var responseObj = Context.Serialization.Deserialize(_rpcQueueMeta.ContentType, 
                commandResult.ResultType, 
                resultBytes);
            
            commandResult.SetResult(responseObj);
        }
        
        // ------------------ RPC Reply Responses -------------------
        
        // Creates a Reply Queue used to receive responses to RPC based commands.
        public async Task CreateEntityAsync(NamespaceConnection connection)
        {
            var options = new CreateQueueOptions(_rpcQueueMeta.ReplyQueueName)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            };

            await connection.AdminClient.CreateQueueAsync(options);
            await SubscribeToReplyQueue(connection);
        }
        
        public async Task CleanupEntityAsync(NamespaceConnection connection)
        {
            if (_replyProcessor != null)
            {
                await _replyProcessor.CloseAsync();
            }
            
            await connection.AdminClient.DeleteQueueAsync(_rpcQueueMeta.ReplyQueueName);
        }
        
        private Task SubscribeToReplyQueue(NamespaceConnection connection)
        {
            _replyProcessor = connection.BusClient.CreateProcessor(_rpcQueueMeta.ReplyQueueName,
                new ServiceBusProcessorOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    MaxConcurrentCalls = 40
                });
            
            _replyProcessor.ProcessMessageAsync += OnMessageReceived;
            _replyProcessor.ProcessErrorAsync += OnProcessingError;
            
            return _replyProcessor.StartProcessingAsync();
        }

        private Task OnMessageReceived(ProcessMessageEventArgs args)
        {
            string correlationId = args.Message.CorrelationId;
                    
            Context.Logger.LogDebug(
                "Received Response for Request with Correlation Id {CorrelationId}.  The current pending" +
                "number of RPC request is {PendingRequests}", correlationId, _pendingRpcRequests.Count);

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                Context.Logger.LogError("The received reply message does not have a CorrelationId.");
                return Task.CompletedTask;
            }

            if (! _pendingRpcRequests.TryRemove(correlationId, out RpcPendingRequest pendingRequest))
            {
                Context.Logger.LogError($"The received correlation Id: {correlationId} does not have pending request.");
                return Task.CompletedTask;       
            }
            
            Context.Logger.LogDebug("Message received on RPC reply queue {QueueName} with Correlation Id {CorrelationId}",
                _rpcQueueMeta.ReplyQueueName, args.Message.CorrelationId);

            SetReplyResult(pendingRequest, args);
            return Task.CompletedTask;
        }

        private static void SetReplyResult(RpcPendingRequest pendingRequest, ProcessMessageEventArgs args)
        {
            var replyEx = CheckReplyException(args);
            if (replyEx != null)
            {
                pendingRequest.SetException(replyEx);
                return;
            }
            
            pendingRequest.SetResult(args.Message.Body.ToArray());
        }

        private static RpcReplyException CheckReplyException(ProcessMessageEventArgs args)
        {
            if (args.Message.ApplicationProperties.TryGetValue("RpcError", out object value))
            {
                return string.IsNullOrEmpty(value.ToString())
                    ? new RpcReplyException("RPC Queue subscriber indicated error without details.")
                    : new RpcReplyException(value.ToString());
            }

            return null;
        }

        private Task OnProcessingError(ProcessErrorEventArgs args)
        {
            Context.LogProcessError(args);
            return Task.CompletedTask;
        }
    }
}