using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Core.Initialization;
using NetFusion.RabbitMQ.Core.Rpc;
using NetFusion.RabbitMQ.Integration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

        // The broker delegates to the base messaging module when received
        // messages need to be dispatched to in-process consumers.
        private readonly IMessagingModule _messagingModule;

        private readonly IContainerLogger _logger;
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

        // External settings and configuration information
        // provided by the plug-in module.
        private MessageBrokerSetup _brokerSetup;
        private IEnumerable<MessageConsumer> _messageConsumers;

        // External managers provided by the plug-in module.
        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;

        // Individual setup classes that are delegated to and contain 
        // implementation for specific publisher or consumer behaviors. 
        private ExchangePublisherSetup _exchangePublisher;
        private ExchangeConsumerSetup _exchangeConsumer;
        private RpcExchangePublisherSetup _rpcExchangePublisher;
        private RpcExchangeConsumerSetup _rpcExchangeConsumer;

        public void Initialize(MessageBrokerSetup brokerSetup)
        {
            Check.NotNull(brokerSetup, nameof(brokerSetup));
            Check.NotNull(brokerSetup.ConnectionMgr, nameof(brokerSetup.ConnectionMgr));
            Check.NotNull(brokerSetup.SerializationMgr, nameof(brokerSetup.SerializationMgr));
            Check.NotNull(brokerSetup.Exchanges, nameof(brokerSetup.Exchanges));
            
            _brokerSetup = brokerSetup;

            _connMgr = brokerSetup.ConnectionMgr;
            _serializationMgr = brokerSetup.SerializationMgr;

            InitializePublishers();
            InitializeConsumers();
        }

        // Initializes the classes that encapsulate the publishing of messages.
        private void InitializePublishers()
        {
            _exchangePublisher = new ExchangePublisherSetup(_logger, _brokerSetup,
                _connMgr,
                _serializationMgr,
                _scriptingSrv);

            _rpcExchangePublisher = new RpcExchangePublisherSetup(_logger, _brokerSetup,
               _connMgr,
               _serializationMgr);
        }

        // Initializes the classes that encapsulate the consuming of messages.
        private void InitializeConsumers()
        {
            _exchangeConsumer = new ExchangeConsumerSetup(_logger, _messagingModule, _brokerSetup,
                _connMgr,
                _serializationMgr);

            _rpcExchangeConsumer = new RpcExchangeConsumerSetup(_logger, _messagingModule, _brokerSetup,
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

            _exchangeConsumer.Dispose();
            _rpcExchangePublisher.Dispose();

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

        public Task PublishToExchangeAsync(IMessage message)
        {
            return _exchangePublisher.PublishToExchangeAsync(message);
        }

        public bool IsRpcCommand(IMessage message)
        {
            return _rpcExchangePublisher.IsRpcCommand(message);
        }

        public Task PublishToRpcConsumerAsync(IMessage message)
        {
            return _rpcExchangePublisher.PublishToRpcConsumerAsync(message);
        }

        private void MonitorForConnectionFailures()
        {
            AttachBokerMonitoringHandlers();
            AttachRpcBrokerMonitoringHandlers();
        }

        // For each RPC Message publisher, monitor the Reply-consumer for unexpected
        // shutdown events and re-declare the RPC clients after a new connection has
        // been established.
        private void AttachRpcBrokerMonitoringHandlers()
        {
            bool handelingShutdown = false;

            IEnumerable<RpcMessagePublisher> monitoringConsumers = _rpcExchangePublisher.RpcMessagePublishers
                .GroupBy(p => p.BrokerName)
                .Select(g => g.First())
                .ToList();

            foreach (RpcMessagePublisher rpcPublisher in monitoringConsumers)
            {
                rpcPublisher.Client.ReplyConsumer.Shutdown += (sender, shutdownEvent) => {

                    if (_connMgr.IsUnexpectedShutdown(shutdownEvent))
                    {
                        handelingShutdown = true;
                        _rpcExchangePublisher.ClearRpcClients(rpcPublisher.BrokerName);
                        _rpcExchangePublisher.DeclareRpcClients(rpcPublisher.BrokerName);
                    }
                };
            }

            // Attach handler to ShutDown event of newly established consumers.
            if (handelingShutdown)
            {
                AttachRpcBrokerMonitoringHandlers();
            }
        }

        // Determine single consumer for each of the unique broker connection.
        // For this unique per-broker consumer, monitor for unexpected shut
        // down events.
        private void AttachBokerMonitoringHandlers()
        {
            IEnumerable<MessageConsumer> monitoringConsumers = _messageConsumers.GroupBy(c => c.BrokerName)
                .Select(g => g.First())
                .ToList();

            foreach (MessageConsumer messageConsumer in monitoringConsumers)
            {
                MessageHandler messageHandler = messageConsumer.MessageHandlers.FirstOrDefault();
                EventingBasicConsumer consumer = messageHandler?.EventConsumer;

                if (consumer != null)
                {
                    consumer.Shutdown += (sender, shutdownEvent) =>
                    {
                        RestablishConnection(messageConsumer, shutdownEvent);
                    };
                }
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
