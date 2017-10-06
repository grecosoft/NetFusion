using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core.Rpc;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for publishing to RPC defined queues defined
    /// by the consuming application.  The consumer awaits the response from
    /// the consumer by specifying an auto-generated queue name on which it 
    /// should publish the response.  If a response is not received within a
    /// specified configured amount of time, an exception is raised.
    /// </summary>
    public class RpcExchangePublisherInit : IBrokerInitializer, IDisposable
    {
        private bool _disposed;
        private object _rpcPublisherLock = new Object();

        private ILogger _logger;
        private MessageBrokerState _brokerState;
        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;

        private IList<RpcMessagePublisher> _rpcMessagePublishers;

        public RpcExchangePublisherInit(
            ILoggerFactory loggerFactory,
            MessageBrokerState brokerState)
        {
            _logger = loggerFactory.CreateLogger<RpcExchangeConsumerInit>();
            _brokerState = brokerState;
            _connMgr = brokerState.ConnectionMgr;
            _serializationMgr = brokerState.SerializationMgr; 

            _rpcMessagePublishers = new List<RpcMessagePublisher>();
        }

        /// <summary>
        /// List of the configured RPC message publishers specifying the RpcClient
        /// that should be used when publishing a message to a RPC consumer queue.
        /// </summary>
        public IEnumerable<RpcMessagePublisher> RpcMessagePublishers => _rpcMessagePublishers;

        /// <summary>
        /// Creates a RpcClient for each configured queue defined by other applications
        /// servers to which messages can be published and a response is expected. The
        /// external RPC queues exposed by other applications are specified within the
        /// configuration file.
        /// </summary>
        /// <param name="brokerName">The optional broker name to create RPC client for.</param>
        public void DeclareRpcClients(string brokerName = null)
        {
            IEnumerable<BrokerConnectionSettings> brokerConnections = _brokerState.BrokerSettings.Connections;

            // The broker name will be null when all the RPC response queues need to be re-created after
            // a connection exception is detected.
            if (brokerName != null)
            {
                brokerConnections = brokerConnections.Where(c => c.BrokerName == brokerName);
            }

            foreach (BrokerConnectionSettings brokerConn in brokerConnections)
            {
                foreach (RpcConsumerSettings rpcConsumer in brokerConn.RpcConsumers)
                {
                    // The channel to await for replays to published messages.
                    IModel replyChannel = _connMgr.CreateChannel(brokerConn.BrokerName);

                    var rpcClient = new RpcClient(brokerConn.BrokerName, rpcConsumer, replyChannel);
                    var rpcPublisher = new RpcMessagePublisher(brokerConn.BrokerName, rpcConsumer, rpcClient);

                    _rpcMessagePublishers.Add(rpcPublisher);
                }
            }
        }

        public void ClearRpcClients(string brokerName)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));

            lock(_rpcPublisherLock)
            {
                IEnumerable<RpcMessagePublisher> rpcPublishers = _rpcMessagePublishers
                    .Where(c => c.BrokerName == brokerName).ToList();

                foreach (RpcMessagePublisher rpcPublisher in rpcPublishers)
                {
                    (rpcPublisher.Client as IDisposable).Dispose();
                    _rpcMessagePublishers.Remove(rpcPublisher);
                }
            }
        }

        /// <summary>
        /// Determines if the specified message meets the requirements for an RPC style command.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if a RPC style message, otherwise, false.</returns>
        public bool IsRpcCommand(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            Type messageType = message.GetType();

            return messageType.IsDerivedFrom<ICommand>()
                && messageType.HasAttribute<RpcCommandAttribute>();
        }

        public void LogDetails(IDictionary<string, object> log)
        {
            var rpcMessages = _brokerState.RpcTypes.Values
                .Select(msgType => {
                    var msgAttrib = msgType.GetAttribute<RpcCommandAttribute>();
                    return new { RpcMessageType = msgType, msgAttrib.BrokerName, msgAttrib.RequestQueueKey };
                });

            // Associate the RPC message command with the publisher that dispatches to the consumer.
            log["RPC Messages"] = (from rpcMsg in rpcMessages
                       join rpcPub in _rpcMessagePublishers on new { rpcMsg.BrokerName, rpcMsg.RequestQueueKey }
                           equals new { rpcPub.BrokerName, rpcPub.RequestQueueKey }
                       select new
                       {
                           rpcMsg.RpcMessageType,
                           rpcPub.BrokerName,
                           rpcPub.RequestQueueKey,
                           rpcPub.RequestQueueName,
                           rpcPub.Client.ReplyQueueName,
                           Channel = MessageBrokerLog.LogChannel(rpcPub.Client.Channel)
                       }).ToDictionary(msgPub => msgPub.RpcMessageType);
        }

        private void AssertRpcCommand(IMessage message)
        {
            if (!IsRpcCommand(message))
            {
                throw new BrokerException(
                    $"The message of type: {message.GetType()} is not a command " +
                    $"or is not decorated with: {typeof(RpcCommandAttribute)}.");
            }
        }

        /// <summary>
        /// Publishes a message to a consumer defined queue used for receiving
        /// RPC style messages.  The caller awaits the reply response message.
        /// </summary>
        /// <param name="message">The RPC style message to publish.</param>
        /// <returns>Future result after the reply is received.</returns>
        public async Task PublishToRpcConsumerAsync(IMessage message, CancellationToken cancellationToken)
        {
            Check.NotNull(message, nameof(message));

            AssertRpcCommand(message);

            var rpcCommandAttrib = message.GetAttribute<RpcCommandAttribute>();
            var command = message as ICommand;

            RpcProperties rpcProps = new RpcProperties {
                ContentType = rpcCommandAttrib.ContentType,
                ExternalTypeName = rpcCommandAttrib.ExternalTypeName
            };

            // Obtain the consumer queue on which the message should be published.
            RpcMessagePublisher rpcPublisher = GetRpcPublisher(rpcCommandAttrib);

            string[] orderedContentTypes = {
                message.GetContentType(),
                rpcProps.ContentType,
                rpcPublisher.ContentType};

            byte[] messageBody = _serializationMgr.Serialize(command, orderedContentTypes);

            LogPublishedRpcMessage(message, rpcPublisher, rpcProps);

            // Publish the RPC request the consumer's queue and await a response.
            rpcProps.ContentType = command.GetContentType();
            byte[] replyBody = null;

            try
            {
                replyBody = await rpcPublisher.Client.Invoke(command, rpcProps, cancellationToken, messageBody);

                object reply = _serializationMgr.Deserialize(rpcProps.ContentType, command.ResultType, replyBody);
                command.SetResult(reply);
                LogReceivedRpcResponse(message, rpcPublisher);
            } 
            catch (RpcReplyException ex)
            {
                var dispatchEx = _serializationMgr.Deserialize<MessageDispatchException>(rpcProps.ContentType, ex.Exception);
                _logger.LogError(RabbitMqLogEvents.PUBLISHER_RPC_RESPONSE, "RPC Exception Reply.", dispatchEx);
                throw dispatchEx;
            }
        }

        private RpcMessagePublisher GetRpcPublisher(RpcCommandAttribute consumerAttrib)
        {
            RpcMessagePublisher rpcPublisher = _rpcMessagePublishers.FirstOrDefault(c =>
                c.BrokerName == consumerAttrib.BrokerName
                && c.RequestQueueKey == consumerAttrib.RequestQueueKey);

            if (rpcPublisher == null)
            {
                throw new BrokerException(
                    $"RPC Publisher Client could not configured for Broker: {consumerAttrib.BrokerName} " +
                    $"RequestQuoteKey: {consumerAttrib.RequestQueueKey}.");
            }
            return rpcPublisher;
        }

        private void LogPublishedRpcMessage(IMessage message,
            RpcMessagePublisher rpcPublisher,
            RpcProperties rpcProps)
        {
            _logger.LogTraceDetails(RabbitMqLogEvents.PUBLISHER_RPC_REQUEST, "Publishing to RPC Consumer", 
                new
                {
                    Message = message,
                    rpcPublisher.BrokerName,
                    rpcPublisher.RequestQueueKey,
                    rpcPublisher.RequestQueueName,
                    rpcPublisher.Client.ReplyQueueName,
                    rpcProps.ContentType,
                    rpcProps.ExternalTypeName
                });
        }

        private void LogReceivedRpcResponse(IMessage message,
            RpcMessagePublisher rpcPublisher)
        {
            _logger.LogTraceDetails(RabbitMqLogEvents.PUBLISHER_RPC_RESPONSE, "RPC Reply Message Received",
                new
                {
                    Message = message,
                    rpcPublisher.BrokerName,
                    rpcPublisher.RequestQueueName,
                    rpcPublisher.Client.ReplyQueueName
                });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;
            if (_rpcMessagePublishers == null) return;

            foreach (RpcMessagePublisher publisher in _rpcMessagePublishers)
            {
                publisher?.Client?.Channel.Dispose();
            }

            _disposed = true;
        }
    }
}
