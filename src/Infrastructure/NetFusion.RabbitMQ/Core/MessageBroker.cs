using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Core.Initialization;
using NetFusion.RabbitMQ.Core.Rpc;
using NetFusion.RabbitMQ.Integration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        // messages need to be dispatched to in-process consumers handlers.
        private readonly IMessagingModule _messagingModule;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IBrokerMetaRepository _exchangeRep;
        private readonly IEntityScriptingService _scriptingSrv;

        public MessageBroker(ILoggerFactory loggerFactory, 
            IMessagingModule messagingModule,
            IBrokerMetaRepository exchangeRep,
            IEntityScriptingService scriptingSrv)
        {
         
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _messagingModule = messagingModule ?? throw new ArgumentNullException(nameof(messagingModule));
            _exchangeRep = exchangeRep ?? throw new ArgumentNullException(nameof(exchangeRep));
            _scriptingSrv = scriptingSrv ?? throw new ArgumentNullException(nameof(scriptingSrv));

            _logger = loggerFactory.CreateLogger<MessageBroker>();
        }

        // External settings and configuration information
        // provided by the plug-in module.
        private MessageBrokerState _brokerState;
        private IEnumerable<MessageConsumer> _messageConsumers;

        // External managers provided by the plug-in module.
        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;

        // Individual setup classes that are delegated to and contain 
        // implementation for specific publisher or consumer behaviors. 
        private ExchangePublisherInit _exchangePublisher;
        private ExchangeConsumerInit _exchangeConsumer;
        private RpcExchangePublisherInit _rpcExchangePublisher;
        private RpcExchangeConsumerInit _rpcExchangeConsumer;
        private IList<IBrokerInitializer> _brokerInitializers;

        public void Initialize(MessageBrokerState brokerState)
        {
            _brokerState = brokerState ?? throw new ArgumentNullException(nameof(brokerState));

            if (brokerState.Exchanges == null) throw new ArgumentNullException(nameof(brokerState.Exchanges),
                "Message Broker must be initialized with Exchanges.");

            _connMgr = brokerState.ConnectionMgr ?? throw new ArgumentNullException(nameof(brokerState.ConnectionMgr));
            _serializationMgr = brokerState.SerializationMgr ?? throw new ArgumentNullException(nameof(brokerState.SerializationMgr));

            _brokerInitializers = new List<IBrokerInitializer>();

            InitializePublishers();
            InitializeConsumers();
        }

        // Initializes the classes that encapsulate the publishing of messages.
        private void InitializePublishers()
        {
            _exchangePublisher = new ExchangePublisherInit(_loggerFactory, _brokerState, _scriptingSrv);
            _rpcExchangePublisher = new RpcExchangePublisherInit(_loggerFactory, _brokerState);

            _brokerInitializers.Add(_exchangePublisher);
            _brokerInitializers.Add(_rpcExchangePublisher);
        }

        // Initializes the classes that encapsulate the consuming of messages.
        private void InitializeConsumers()
        {
            _exchangeConsumer = new ExchangeConsumerInit(_loggerFactory, _messagingModule, _brokerState);
            _rpcExchangeConsumer = new RpcExchangeConsumerInit(_loggerFactory, _messagingModule, _brokerState);

            _brokerInitializers.Add(_exchangeConsumer);
            _brokerInitializers.Add(_rpcExchangeConsumer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;

            foreach(IBrokerInitializer initializer in _brokerInitializers)
            {
                (initializer as IDisposable)?.Dispose();
            }

            _disposed = true;
        }

        public void ConfigurePublishers()
        {
            _connMgr.EstablishBrockerConnections();

            _exchangePublisher.DeclareExchanges();
            _rpcExchangePublisher.DeclareRpcClients();
        }

        public void BindConsumers(IEnumerable<MessageConsumer> messageConsumers)
        {
            _messageConsumers = messageConsumers ?? throw new ArgumentNullException(nameof(messageConsumers));

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

        public Task PublishToRpcConsumerAsync(IMessage message, CancellationToken cancellationToken)
        {
            return _rpcExchangePublisher.PublishToRpcConsumerAsync(message, cancellationToken);
        }

        private void MonitorForConnectionFailures()
        {
           AttachBokerMonitoringHandlers();
           AttachRpcBrokerMonitoringHandlers();
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

        // For each RPC Message publisher, monitor the Reply-consumer for unexpected
        // shutdown events and re-declare the RPC clients after a new connection has
        // been established.
        private void AttachRpcBrokerMonitoringHandlers()
        {
            IEnumerable<RpcMessagePublisher> monitoringConsumers = _rpcExchangePublisher.RpcMessagePublishers
                .GroupBy(p => p.BrokerName)
                .Select(g => g.First())
                .ToList();

            foreach (RpcMessagePublisher rpcPublisher in monitoringConsumers)
            {
                rpcPublisher.Client.ReplyConsumer.Shutdown += (sender, shutdownEvent) => {

                    if (_connMgr.IsUnexpectedShutdown(shutdownEvent))
                    {
                        _rpcExchangePublisher.ClearRpcClients(rpcPublisher.BrokerName);
                        _rpcExchangePublisher.DeclareRpcClients(rpcPublisher.BrokerName);

                        // Attach handler to ShutDown event of newly established consumers.
                        AttachRpcBrokerMonitoringHandlers();
                    }
                };
            }
        }

        public void LogDetails(IDictionary<string, object> log)
        {
            foreach(IBrokerInitializer initializer in _brokerInitializers)
            {
                initializer.LogDetails(log);
            }
        }

        //------------------------------------------BROKER RECONNECTION------------------------------------------//

        // Called when a channel is notified of an connection issue.  Once the connection is
        // reestablished, the centrally saved exchanges and queues need to be recreated on the
        // new connection.  This is important when using a broker fail-over behind a load-balancer
        // since the backup broker my not have the exchanges.
        private void RestablishConnection(MessageConsumer messageConsumer, ShutdownEventArgs shutdownEvent)
        {
            if (_connMgr.IsBrokerConnected(messageConsumer.BrokerName)) return;

            if (_connMgr.IsUnexpectedShutdown(shutdownEvent))
            {
                _logger.LogError(RabbitMqLogEvents.BROKER_CONFIGURATION, "Connection to broker was shutdown.  Reconnection will be attempted.");

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

                _logger.LogDebug(RabbitMqLogEvents.BROKER_CONFIGURATION,  "Connection to broker was reestablished.");
            }
        }

        private void ReCreatePublisherExchanges(IModel channel, IEnumerable<BrokerMeta> brokerMeta)
        {
            IEnumerable<ExchangeMeta> exchanges = brokerMeta.SelectMany(b => b.ExchangeMeta)
                .Where(e => e.Settings != null)
                .ToList();

            ReCreateExchanges(channel, exchanges);

            _logger.LogDebug(RabbitMqLogEvents.PUBLISHER_CONFIGURATION, "Publisher exchanges recreated after broker connection failure.");
        }

        private void ReCreateConsumerQueues(IModel channel, IEnumerable<BrokerMeta> brokerMeta)
        {
            IEnumerable<ExchangeMeta> exchanges = brokerMeta.SelectMany(b => b.ExchangeMeta)
                .Where(e => e.Settings == null || e.Settings.IsConsumerExchange)
                .ToList();

            ReCreateExchanges(channel, exchanges);

            _logger.LogDebug(RabbitMqLogEvents.CONSUMER_CONFIGURATION, "Consumer queues recreated after broker connection failure.");
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
    }
}
