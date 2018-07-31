using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Manages the publishing of an RPC style message and correlates the response
    /// received in the reply queue back to the orginating command.
    /// </summary>
    internal class RpcClient : IRpcClient
    {
        private readonly string _busName;
        private ILogger _logger;

        // Dictionary containing the pending task associated with the orginating 
        // sent outgoing command keyed by correlation id.  
        private readonly ConcurrentDictionary<string, RpcPendingRequest> _pendingRpcRequests;

        public IBus Bus { get; }
        public string ReplyToQueueName { get; }

        public RpcClient(string busName, string exchangeName, IBus bus)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name not specified.", nameof(exchangeName));

            _busName = busName ?? throw new ArgumentNullException(nameof(busName));
            _pendingRpcRequests = new ConcurrentDictionary<string, RpcPendingRequest>();

            Bus = bus ?? throw new ArgumentNullException(nameof(bus));

            // The queue unique to the running host instance on which replies to published
            // commands will be received.
            ReplyToQueueName = $"RPC_{exchangeName}_{Guid.NewGuid()}";
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<byte[]> Publish(CreatedExchange createdExchange, byte[] messageBody,
            MessageProperties messageProperties,
            CancellationToken cancellationToken)
        {
            if (createdExchange == null) throw new ArgumentNullException(nameof(createdExchange));
            if (messageBody == null) throw new ArgumentNullException(nameof(messageBody));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            AppendRpcMessageProperties(messageProperties);

            // Create a task that can be completed in the future when the result
            // is received in the reply queue. 
            var futureResult = new TaskCompletionSource<byte[]>();

            string correlationId = messageProperties.CorrelationId;
            int cancelRpcRequestAfterMs = createdExchange.Definition.CancelRpcRequestAfterMs;

            var rpcPendingRequest = new RpcPendingRequest(futureResult, cancellationToken, cancelRpcRequestAfterMs);
            
            _pendingRpcRequests[correlationId] = rpcPendingRequest;

            // Publish command to the Rpc exchange.  A Direct exchange is being used for RPC messages
            // where the route key is used to identity the command and used by the consumer to call
            // the correct handler.
            string routeKey = createdExchange.Definition.RouteKey;

            // Publish the command to the exchange.
            await createdExchange.Bus.Advanced.PublishAsync(createdExchange.Exchange, 
                routeKey, 
                false,
                messageProperties,
                messageBody);

            try
            {
                // This task will be completed when the response to the command is received on
                // on the reply queue.  The task result is the bytes of the response message.
                return await futureResult.Task;
            }
            catch (TaskCanceledException ex)
            {
                // If the pending request task has been canceled, remove the pending
                // request and unregister the cancellation delegate.
                if (_pendingRpcRequests.TryRemove(correlationId, out RpcPendingRequest pendingRequest))
                {
                    pendingRequest.UnRegister();
                }

                throw new RpcReplyException(
                    $"The RPC request with the correlation value of: {correlationId} was canceled. " + 
                    $"The current timeout value is: {cancelRpcRequestAfterMs} ms.", 
                    ex);
            }
            catch (Exception ex)
            {
                if (_pendingRpcRequests.TryRemove(correlationId, out RpcPendingRequest pendingRequest))
                {
                    pendingRequest.UnRegister();
                }
                throw new RpcReplyException(
                    "The RPC request with the correlation value of: {correlationId} resulted in an exception.", ex);
            }
        }

        // Add properties used by the receiving consumer required for sending 
        // the response to the reply queue.
        private void AppendRpcMessageProperties(MessageProperties msgProps)
        {
            msgProps.ReplyTo = ReplyToQueueName;
            msgProps.CorrelationId = msgProps.CorrelationId ?? Guid.NewGuid().ToString();
            msgProps.SetRpcReplyBusConfigName(_busName);
        }

        public void CreateAndSubscribeToReplyQueue()
        {
            var queue = Bus.Advanced.QueueDeclare(ReplyToQueueName,
                passive: false,
                durable: false,
                exclusive: true,
                autoDelete: true);

            // Consumes the reply queue and determines the pending task associated with the
            // orginating sent command and sets the result to mark the task completed.
            Bus.Advanced.Consume(queue, 
                (msgBody, msgProps, msgReceiveInfo) => {

                    string correlationId = msgProps.CorrelationId;

                    if (string.IsNullOrWhiteSpace(correlationId))
                    {
                        _logger.LogError("The received reply message does not have a CorrelationId.");
                    }

                    if (! _pendingRpcRequests.TryRemove(correlationId, out RpcPendingRequest pendingRequest))
                    {
                        _logger.LogError(
                            $"The received correlation Id: {correlationId} does not have pending request.");
                        
                        return;        
                    }
                    
                    _logger.LogTrace("Reply to message with correlation id of: {correlationId} received.", correlationId);

                    if (msgProps.IsRpcReplyException())
                    {
                        pendingRequest.SetException(msgBody);
                        return;
                    }

                    pendingRequest.SetResult(msgBody);
                });
        }
    }
}