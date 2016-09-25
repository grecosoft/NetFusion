using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Core.Initialization;
using NetFusion.RabbitMQ.Core.Rpc;
using NetFusion.RabbitMQ.Integration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Coordinates the create of exchanges, queues, and consumers
    /// by delegating to the corresponding initialization classes.
    /// </summary>
    public class MessageBroker: IDisposable,
        IMessageBroker
    {
        private bool _disposed;

        private readonly IContainerLogger _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly IBrokerMetaRepository _exchangeRep;
        private readonly IEntityScriptingService _scriptingSrv;

        public MessageBroker(IContainerLogger logger, 
            IMessagingModule messagingModule,
            IBrokerMetaRepository exchangeRep,
            IEntityScriptingService scriptingSrv)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(messagingModule, nameof(messagingModule));
            Check.NotNull(exchangeRep, nameof(exchangeRep));
            Check.NotNull(scriptingSrv, nameof(scriptingSrv));

            _logger = logger.ForPluginContext<MessageBroker>();
            _messagingModule = messagingModule;
            _exchangeRep = exchangeRep;
            _scriptingSrv = scriptingSrv;
        }

        private MessageBrokerConfig _brokerConfig;
        private IEnumerable<MessageConsumer> _messageConsumers;

        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;
        private ExchangePublisherSetup _exchangePublisher;
        private ExchangeConsumerSetup _exchangeConsumer;
        private RpcExchangePublisherSetup _rpcExchangePublisher;
        private RpcExchangeConsumerSetup _rpcExchangeConsumer;

        public void Initialize(MessageBrokerConfig brokerConfig)
        {
            Check.NotNull(brokerConfig, nameof(brokerConfig));
            Check.NotNull(brokerConfig.ConnectionMgr, nameof(brokerConfig.ConnectionMgr));
            Check.NotNull(brokerConfig.SerializationMgr, nameof(brokerConfig.SerializationMgr));
            Check.NotNull(brokerConfig.Exchanges, nameof(brokerConfig.Exchanges));
            
            _brokerConfig = brokerConfig;

            _connMgr = brokerConfig.ConnectionMgr;
            _serializationMgr = brokerConfig.SerializationMgr;

            InitializePublishers();
            InitializeConsumers();
        }

        private void InitializePublishers()
        {
            _exchangePublisher = new ExchangePublisherSetup(_logger, _brokerConfig,
                _connMgr,
                _serializationMgr,
                _scriptingSrv);

            _rpcExchangePublisher = new RpcExchangePublisherSetup(_logger, _brokerConfig,
               _connMgr,
               _serializationMgr);
        }

        private void InitializeConsumers()
        {
            _exchangeConsumer = new ExchangeConsumerSetup(_logger, _messagingModule, _brokerConfig,
                _connMgr,
                _serializationMgr);

            _rpcExchangeConsumer = new RpcExchangeConsumerSetup(_logger, _messagingModule, _brokerConfig,
                _connMgr,
                _serializationMgr);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;

            (_connMgr as IDisposable)?.Dispose();
            _disposed = true;
        }

        public void ConfigureBroker()
        {
            _connMgr.EstablishBrockerConnections();
            _exchangePublisher.DeclareExchanges();
            _rpcExchangePublisher.DeclareRpcClients();
        }

        public void BindConsumers(IEnumerable<MessageConsumer> messageConsumers)
        {
            Check.NotNull(messageConsumers, nameof(messageConsumers));

            _messageConsumers = messageConsumers;

            _exchangeConsumer.BindConsumersToQueues(messageConsumers);
            _rpcExchangeConsumer.BindConsumersToRpcQueues();

            MonitorForConnectionFailures();
        }

        public bool IsExchangeMessage(IMessage message)
        {
            return _exchangePublisher.IsExchangeMessage(message);
        }

        public Task PublishToExchange(IMessage message)
        {
            return _exchangePublisher.PublishToExchange(message);
        }

        public bool IsRpcCommand(IMessage message)
        {
            return _rpcExchangePublisher.IsRpcCommand(message);
        }

        public Task PublishToRpcConsumer(IMessage message)
        {
            return _rpcExchangePublisher.PublishToRpcConsumer(message);
        }

        private void MonitorForConnectionFailures()
        {
            AttachBokerMonitoringHandlers();
            AttachRpcBrokerMonitoringHandlers();
        }

        private void AttachRpcBrokerMonitoringHandlers()
        {
            foreach(RpcMessagePublisher rpcPublisher in _rpcExchangePublisher.RpcMessagePublishers)
            {
                rpcPublisher.Client.Consumer.Shutdown += (sender, shutdownEvent) => {

                    if (_connMgr.IsUnexpectedShutdown(shutdownEvent))
                    {
                        _connMgr.ReconnectToBroker(rpcPublisher.BrokerName);
                        _rpcExchangePublisher.RemoveRpcBrokerPublishers(rpcPublisher.BrokerName);
                        _rpcExchangePublisher.DeclareRpcClients(rpcPublisher.BrokerName);
                    }
                };
            }
        }

        // Determine a consumer for each of the connected brokers and attach an event
        // handler to monitor the connection for any failures.
        private void AttachBokerMonitoringHandlers()
        {
            IEnumerable<MessageConsumer> monitoringConsumers = _messageConsumers.GroupBy(c => c.BrokerName)
                .Select(g => g.First())
                .ToList();

            foreach (MessageConsumer consumer in monitoringConsumers)
            {
                consumer.Consumer.Shutdown += (sender, shutdownEvent) =>
                {
                    RestablishConnection(consumer, shutdownEvent);
                };
            }
        }

        #region Broker Reconnection

        // Called when a channel is notified of an connection issue.  Once the connection is
        // reestablished, the centrally saved exchanges and queues need to be recreated on the
        // new connection.  This is important when using a broker fail-over behind a load-balancer
        // since the backup broker my not have the exchanges.
        private void RestablishConnection(MessageConsumer messageConsumer, ShutdownEventArgs shutdownEvent)
        {
            if (_connMgr.IsBrokerConnected(messageConsumer.BrokerName)) return;

            if (_connMgr.IsUnexpectedShutdown(shutdownEvent))
            {
                _logger.Error("Connection to broker was shutdown.  Reconnection will be attempted.");

                string brokerName = messageConsumer.BrokerName;
                IEnumerable<BrokerMeta> brokerMeta = _exchangeRep.LoadAsync(brokerName).Result;

                _connMgr.ReconnectToBroker(brokerName);

                // Restore the exchanges and queues on what might be a new broker.
                using (IModel channel = _connMgr.CreateChannel(brokerName))
                {
                    ReCreatePublisherExchanges(channel, brokerMeta);
                    ReCreateConsumerQueues(channel, brokerMeta);
                    _exchangeConsumer.BindConsumersToQueues(_messageConsumers, messageConsumer.BrokerName);

                    _rpcExchangeConsumer.BindConsumersToRpcQueues();
                }

                // Watch for future issues.
                AttachBokerMonitoringHandlers();

                _logger.Debug("Connection to broker was reestablished.");
            }
        }

        private void ReCreatePublisherExchanges(IModel channel, IEnumerable<BrokerMeta> brokerMeta)
        {
            IEnumerable<ExchangeMeta> exchanges = brokerMeta.SelectMany(b => b.ExchangeMeta)
                .Where(e => e.Settings != null)
                .ToList();

            ReCreateExchanges(channel, exchanges);

            _logger.Debug("Publisher exchanges recreated after broker connection failure.");
        }

        private void ReCreateConsumerQueues(IModel channel, IEnumerable<BrokerMeta> brokerMeta)
        {
            IEnumerable<ExchangeMeta> exchanges = brokerMeta.SelectMany(b => b.ExchangeMeta)
                .Where(e => e.Settings == null || e.Settings.IsConsumerExchange)
                .ToList();

            ReCreateExchanges(channel, exchanges);

            _logger.Debug("Consumer queues recreated after broker connection failure.");
        }

        private void ReCreateExchanges(IModel channel, IEnumerable<ExchangeMeta> exchanges)
        {
            foreach (ExchangeMeta exchange in exchanges)
            {
                // Recreate the exchange if not a default exchange.
                if (exchange.Settings != null && exchange.Settings.ExchangeType != null)
                {
                    channel.ExchangeDeclare(exchange.Settings);
                }

                foreach (var queue in exchange.QueueMeta)
                {
                    channel.QueueDeclare(queue.QueueName, queue.Settings);
                }
            }
        }

        #endregion
    }
}
