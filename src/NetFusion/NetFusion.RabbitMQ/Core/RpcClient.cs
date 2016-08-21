using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    public class RpcClient : IRpcClient
    {
        private const string DEFAULT_EXCHANGE = "";

        private readonly IModel _channel;
        private readonly string _rpcRequestQueueName;
        private readonly string _replyQueueName;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _futureResults;
        private readonly EventingBasicConsumer _consumer;

        public RpcClient(string rpcRequestQueueName, IModel channel)
        {
            Check.NotNull(rpcRequestQueueName, nameof(rpcRequestQueueName));
            Check.NotNull(channel, nameof(channel));

            _channel = channel;
            _rpcRequestQueueName = rpcRequestQueueName;
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _futureResults = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
            _consumer = new EventingBasicConsumer(channel);

            _channel.BasicConsume(_replyQueueName, true, _consumer);
            _consumer.Received += HandleReplyResponse;
        }

        public async Task<byte[]> Invoke(ICommand command, byte[] messageBody,
            IModel publishChannel)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(messageBody, nameof(messageBody));

            // Associate a correlation value with the outgoing message.
            string correlationId = Guid.NewGuid().ToString();
            command.SetCorrelationId(correlationId);

            // Create a future task that can be resolved in the future when the
            // result is received from the queue.
            var futureResult = new TaskCompletionSource<byte[]>();
            _futureResults[correlationId] = futureResult;

            IBasicProperties basicProps = GetBasicProperties(command);

            publishChannel.BasicPublish(DEFAULT_EXCHANGE,
                             _rpcRequestQueueName,
                             basicProps,
                             messageBody);

            return await futureResult.Task;
        }

        private IBasicProperties GetBasicProperties(ICommand command)
        {
            var rptAttrib = command.GetAttribute<RpcCommandAttribute>();

            IBasicProperties props = _channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.ContentType = "application/json; charset=utf-8";
            props.CorrelationId = command.GetCorrelationId();
            props.Type = rptAttrib.ExternalTypeName;
            return props;
        }

        // When a response is received within the queue for a prior request,
        // find the corresponding future-result for the received correlation
        // value and satisfy the result.
        private void HandleReplyResponse(object sender, BasicDeliverEventArgs evt)
        {
            TaskCompletionSource<byte[]> futureResult = null;

            if (_futureResults.TryRemove(evt.BasicProperties.CorrelationId, out futureResult))
            {
                futureResult.SetResult(evt.Body);
            }
        }
    }
}
