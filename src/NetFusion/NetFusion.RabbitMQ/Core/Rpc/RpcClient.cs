using NetFusion.Common;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Configs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Rpc
{
    /// <summary>
    /// Responsible for making RPC requests and returning the response when
    /// received on the reply queue.  The response is correlated with the
    /// made request.  If the response is not received within a specific 
    /// amount of time, the pending request is canceled.
    /// </summary>
    public class RpcClient : IRpcClient, IDisposable
    {
        private const string DEFAULT_EXCHANGE = "";
        public const string RPC_BROKER_NAME = "broker-name";
        public const string RPC_HEADER_EXCEPTION_INDICATOR = "IsRpcReplyException";

        public IModel Channel { get; }
        public string ReplyQueueName { get; }
        public EventingBasicConsumer ReplyConsumer { get; }

        private readonly string _brokerName;
        private readonly RpcConsumerSettings _consumerSettings;
 
        // Correlation Value to Pending Request Mapping:
        private readonly ConcurrentDictionary<string, RpcPendingRequest> _pendingRpcRequests;
 
        public RpcClient(string brokerName, RpcConsumerSettings consumerSettings, IModel channel)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));
            Check.NotNull(consumerSettings, nameof(consumerSettings)); 
            Check.NotNull(channel, nameof(channel));

            _brokerName = brokerName;
            _consumerSettings = consumerSettings;

            this.Channel = channel;
            this.ReplyConsumer = new EventingBasicConsumer(channel);
            this.ReplyQueueName = this.Channel.QueueDeclare().QueueName;

            // Pending requests by correlation value.
            _pendingRpcRequests = new ConcurrentDictionary<string, RpcPendingRequest>();
            
            ConsumeReplyQueue();
        }

        // Process the consumer's replays to submitted requests.
        private void ConsumeReplyQueue()
        {
            this.Channel.BasicConsume(this.ReplyQueueName, true, this.ReplyConsumer);
            this.ReplyConsumer.Received += HandleReplyResponse;
        }

        public async Task<byte[]> Invoke(ICommand command, RpcProperties rpcProps, 
            byte[] messageBody)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(rpcProps, nameof(rpcProps));
            Check.NotNull(messageBody, nameof(messageBody));

            // Associate a correlation value with the outgoing message.
            string correlationId = Guid.NewGuid().ToString();
            command.SetCorrelationId(correlationId);

            // Create a task that can be resolved in the future when the result
            // is received from the reply queue. 
            var futureResult = new TaskCompletionSource<byte[]>();
            var rpcPendingRequest = new RpcPendingRequest(futureResult, _consumerSettings.CancelRequestAfterMs);

            _pendingRpcRequests[correlationId] = rpcPendingRequest;

            IBasicProperties basicProps = GetRequestProperties(command, rpcProps);

            this.Channel.BasicPublish(DEFAULT_EXCHANGE, _consumerSettings.RequestQueueName,
                basicProps,
                messageBody);

            try
            {
                return await futureResult.Task;
            }
            catch (TaskCanceledException ex)
            {
                // If the pending request task has been canceled, remove the pending
                // request and unregister the cancellation delegate.
                RpcPendingRequest pendingRequest = null;
                if (_pendingRpcRequests.TryRemove(correlationId, out pendingRequest))
                {
                    pendingRequest.UnRegister();
                }

                throw new BrokerException(
                    $"The RPC request with the correlation value of: {correlationId} was canceled. " + 
                    $"The current timeout value is: {_consumerSettings.CancelRequestAfterMs} ms.", 
                    ex);
            }
        }

        private IBasicProperties GetRequestProperties(ICommand command, RpcProperties rpcProps)
        {
            IBasicProperties props = this.Channel.CreateBasicProperties();

            props.Headers = new Dictionary<string, object> { { RPC_BROKER_NAME, _brokerName } };
            props.ReplyTo = this.ReplyQueueName;
            props.CorrelationId = command.GetCorrelationId();
            props.ContentType = rpcProps.ContentType;
            props.Type = rpcProps.ExternalTypeName;
            
            return props;
        }

        // When a response is received within the reply queue for a prior request,
        // find the corresponding pending request for the received correlation
        // value and satisfy the result with the reply this will allow the caller
        // that is awaiting a reply to continue.
        private void HandleReplyResponse(object sender, BasicDeliverEventArgs evt)
        {
            RpcPendingRequest pendingRequest = null;
            string correlationId = evt.BasicProperties.CorrelationId;

            if (_pendingRpcRequests.TryRemove(correlationId, out pendingRequest))
            {
                if (evt.BasicProperties.IsRpcReplyException())
                {
                    pendingRequest.SetException(evt.Body);
                    return;
                }

                pendingRequest.SetResult(evt.Body);
            }
        }

        public void Dispose()
        {
            foreach (RpcPendingRequest pendingRequest in _pendingRpcRequests.Values)
            {
                pendingRequest.Cancel();
            }
        }
    }
}
