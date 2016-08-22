using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Integration;
using NetFusion.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Processes message exchange definitions and creates an exchange for each
    /// definition.  A given exchange definition can also specify any queues to be
    /// created along with the exchange.  Having these queues created along with the
    /// exchange will allow any published messages to be delivered to the queue and
    /// then processed when consumers are connected.  This class also implements the
    /// process for joining consumers to existing queues and for creating new queues
    /// specific to a consumer.  When messages are received, the Messaging Module is
    /// delegated to for dispatching the message to the associated message handlers.
    /// </summary>
    public class MessageBroker: IDisposable,
        IMessageBroker
    {
        private bool _disposed;
        private readonly IContainerLogger _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly IBrokerMetaRepository _exchangeRep;
        private readonly IEntityScriptingService _scriptingSrv;

        private MessageBrokerConfig _brokerConfig;
        private ILookup<Type, ExchangeDefinition> _messageExchanges;
        private IList<RpcMessageConsumer> _rpcMessageConsumers;
        private IEnumerable<MessageConsumer> _messageConsumers;

        public MessageBroker(IContainerLogger logger, 
            IMessagingModule messagingModule,
            IBrokerMetaRepository exchangeRep,
            IEntityScriptingService scriptingSrv)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(messagingModule, nameof(messagingModule));
            Check.NotNull(exchangeRep, nameof(exchangeRep));
            Check.NotNull(scriptingSrv, nameof(scriptingSrv));

            _logger = logger.ForContext<MessageBroker>();
            _messagingModule = messagingModule;
            _exchangeRep = exchangeRep;
            _scriptingSrv = scriptingSrv;
            _rpcMessageConsumers = new List<RpcMessageConsumer>();
        }

        /// <summary>
        /// Initializes new broker.
        /// </summary>
        /// <param name="brokerConfig">Initialization properties specified by owning module.</param>
        public void  Initialize(MessageBrokerConfig brokerConfig)
        {
            Check.NotNull(brokerConfig.Connections, nameof(brokerConfig.Connections));
            Check.NotNull(brokerConfig.Exchanges, nameof(brokerConfig.Exchanges));
            Check.NotNull(brokerConfig.Serializers, nameof(brokerConfig.Serializers));

            _brokerConfig = brokerConfig;

            // Messages can have one or more associated exchanges.
            _messageExchanges = brokerConfig.Exchanges.ToLookup(
                k => k.MessageType,
                e => new ExchangeDefinition(e, e.MessageType));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;

            DisposeConnections();
            _disposed = true;
        }

        private void DisposeConnections()
        {
            _messageConsumers.ForEach(c => c.Channel?.Dispose());
            _brokerConfig.Connections.ForEachValue(bc => bc.Connection.Dispose());
        }

        /// <summary>
        /// Determines if the message is associated with an exchange.
        /// </summary>
        /// <param name="message">The message to lookup.</param>
        /// <returns>True if the message has one or more corresponding exchanges.
        /// Otherwise, False is returned.</returns>
        public bool IsExchangeMessage(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            return _messageExchanges.Contains(message.GetType());
        }

        /// <summary>
        /// Creates all of the exchanges to which messages can be published.  Any default queue
        /// configurations specified by the exchange will also be created.
        /// </summary>
        public void ConfigureBroker()
        {
            EstablishBrockerConnections();

            DeclareExchanges();
            CreateRpcMessageConsumers();
        }

        private void EstablishBrockerConnections()
        {
            foreach (BrokerConnection brokerConn in _brokerConfig.Connections.Values)
            {
                ConnectToBroker(brokerConn);
            }
        }

        protected virtual void ConnectToBroker(BrokerConnection brokerConn)
        {
            IConnectionFactory connFactory = CreateConnFactory(brokerConn);

            try
            {
                brokerConn.Connection = connFactory.CreateConnection();
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.Error("Error connecting to broker.", ex,
                    new
                    {
                        brokerConn.BrokerName,
                        brokerConn.HostName,
                        brokerConn.UserName
                    });

                MethodInvoker.TryCallFor<BrokerUnreachableException>(
                    _brokerConfig.Settings.ConnectionRetryDelayMs,
                    _brokerConfig.Settings.NumConnectionRetries,

                    () => brokerConn.Connection = connFactory.CreateConnection());
            }
        }

        private IConnectionFactory CreateConnFactory(BrokerConnection brokerConn)
        {
            IDictionary<string, object> clientProps = AppendConnectionProperties(
                _brokerConfig.ClientProperties, brokerConn);

            return new ConnectionFactory
            {
                HostName = brokerConn.HostName,
                UserName = brokerConn.UserName,
                Password = brokerConn.Password,
                VirtualHost = brokerConn.VHostName,
                ClientProperties = clientProps
            };
        }

        private IDictionary<string, object> AppendConnectionProperties(
            IDictionary<string, object> clientProperties,
            BrokerConnection brokerConn)
        {
            var props = new Dictionary<string, object>(clientProperties);
            props["Broker Name"] = brokerConn.BrokerName;
            props["Broker User"] = brokerConn.UserName;
            props["Time Connected"] = DateTime.Now.ToString();

            return props;
        }

        private IModel CreateBrokerChannel(string brokerName)
        {
            BrokerConnection brokerConn = _brokerConfig.Connections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new InvalidOperationException(
                    $"Channel could not be created.  A broker with the name of: {brokerName} does not exist.");
            }

            IModel channel = null;
            try
            {
                channel = brokerConn.Connection.CreateModel();
            }
            catch (BrokerUnreachableException)
            {
                ConnectToBroker(brokerConn);
                channel = brokerConn.Connection.CreateModel();
            }
            catch (AlreadyClosedException)
            {
                ConnectToBroker(brokerConn);
                channel = brokerConn.Connection.CreateModel();
            }

            return channel;
        }

        // These are the message exchanges and associated queues defined
        // in the running application to which it will publish messages.
        private void DeclareExchanges()
        {
            ExchangeDefinition[] exchangeDefs = GetExchangeDefinitions();
            foreach (ExchangeDefinition exDef in exchangeDefs)
            {
                using (IModel channel = CreateBrokerChannel(exDef.Exchange.BrokerName))
                {
                    exDef.Exchange.Declare(channel);
                }
            }
        }

        private ExchangeDefinition[] GetExchangeDefinitions()
        {
            return _messageExchanges.Values().ToArray();
        }

        // These are the application's defined consumer queues defined
        // by other application servers to which it can publish commands
        // that will return a near immediate replay.
        private void CreateRpcMessageConsumers()
        {
            foreach (BrokerConnection brokerConn in _brokerConfig.Settings.Connections)
            {
                foreach (RpcConsumerSettings consumer in brokerConn.RpcConsumers)
                {
                    IModel replyChannel = CreateBrokerChannel(brokerConn.BrokerName);

                    var rpcClient = new RpcClient(consumer.RequestQueueName, replyChannel);
                    var messageConsumer = new RpcMessageConsumer(brokerConn.BrokerName, consumer, rpcClient);

                    _rpcMessageConsumers.Add(messageConsumer);
                }
            }
        }

        // Uses the dispatch metadata of the core messaging plugin-in and
        // subscribes and dispatches to the event handler(s) that should 
        // process the message when received.
        public void BindConsumers(IEnumerable<MessageConsumer> messageConsumers)
        {
            Check.NotNull(messageConsumers, nameof(messageConsumers));

            _messageConsumers = messageConsumers;
            BindConsumersToQueues();
            BindConsumersToRpcQueues();
            AttachBokerMonitoringHandlers();
        }

        private void BindConsumersToQueues(string brokerName = null)
        {
            IEnumerable<MessageConsumer> messageConsumers = brokerName == null ? _messageConsumers :
                _messageConsumers.Where(c => c.BrokerName == brokerName);

            foreach (MessageConsumer messageConsumer in messageConsumers)
            {
                CreateConsumerQueue(messageConsumer);

                IModel consumerChannel = CreateBrokerChannel(messageConsumer.BrokerName);
                messageConsumer.Channel = consumerChannel;

                // Bind to the existing or newly created queue to the exchange
                // if the default exchange is not specified.
                if (! messageConsumer.ExchangeName.IsNullOrWhiteSpace())
                {
                    _brokerConfig.Settings.ApplyQueueSettings(messageConsumer);
                    consumerChannel.QueueBind(messageConsumer);
                }

                consumerChannel.SetBasicConsumer(messageConsumer);
                AttachConsumerHandlers(messageConsumer);
            }
        }

        // Consuming applications can create queues specifically for them based on the
        // application needs.  
        private void CreateConsumerQueue(MessageConsumer eventConsumer)
        {
            if (eventConsumer.BindingType == QueueBindingTypes.Create)
            {
                using (IModel channel = CreateBrokerChannel(eventConsumer.BrokerName))
                {
                    channel.QueueDeclare(eventConsumer);
                }
            }
        }

        // Process queue messages when they are received.
        private void AttachConsumerHandlers(MessageConsumer messageConsumer)
        {
            messageConsumer.Consumer.Received += (sender, deliveryEvent) => {

                MessageReceived(messageConsumer, deliveryEvent);
            };
        }

        // Binds the consumer to its RPC queues on which they wait for RPC
        // style messages from publishers.  
        private void BindConsumersToRpcQueues()
        {
            var rpcConsumers = GetExchangeDefinitions()
                .Where(ed => ed.Exchange.Settings.IsConsumerExchange)
                .SelectMany(ed => ed.Exchange.Queues, 
                    (ed, q) => new {
                        BrokerName = ed.Exchange.BrokerName,
                        RpcQueue = q
                    }).ToList();

            foreach (var rpcConsumer in rpcConsumers)
            {
                IModel consumerChannel = CreateBrokerChannel(rpcConsumer.BrokerName);
                EventingBasicConsumer consumer = consumerChannel.SetBasicConsumer(rpcConsumer.RpcQueue);
                AttachRpcConsumerHandler(consumerChannel, consumer);
            }
        }

        private void AttachRpcConsumerHandler(IModel channel, EventingBasicConsumer consumer)
        {
            consumer.Received += (sender, deliveryEvent) => {

                RpcMessageReceived(channel, deliveryEvent);
            };
        }

        // Called when a RPC style message is received on a queue defined by a 
        // derived RpcExchange class that receives RPC style requests.
        private void RpcMessageReceived(IModel channel, BasicDeliverEventArgs deleveryEvent)
        {
            ValidateRpcRequest(deleveryEvent);

            string typeName = deleveryEvent.BasicProperties.Type;
            Type commandType = _brokerConfig.RpcTypes[typeName];
            MessageDispatchInfo dispatcher = _messagingModule.GetInProcessCommandDispatcher(commandType);
            IMessage message = DeserializeMessage(commandType, deleveryEvent);

            object result =_messagingModule.InvokeDispatcher(dispatcher, message).Result;
            PublishReply(result, channel, deleveryEvent.BasicProperties);
        }

        private void ValidateRpcRequest(BasicDeliverEventArgs deleveryEvent)
        {
            string messageType = deleveryEvent.BasicProperties.Type;

            if (messageType.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    "The basic properties of the received RPC request does not specify the message type.");
            }

            if (! _brokerConfig.RpcTypes.ContainsKey(messageType))
            {
                throw new InvalidOperationException(
                    $"The type associated with the message type name: {messageType} could not be resolved.");
            }
        }

        // Publish the reply back to the publisher that made the request.
        private void PublishReply(object response, IModel channel, IBasicProperties requestProps)
        {
            byte[] replyBody = SerializeMessage(response, requestProps.ContentType);
            IBasicProperties replyProps = channel.CreateBasicProperties();

            replyProps.ContentType = requestProps.ContentType;
            replyProps.CorrelationId = requestProps.CorrelationId;

            channel.BasicPublish(exchange: "",
                    routingKey: requestProps.ReplyTo,
                    basicProperties: replyProps,
                    body: replyBody);

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
                consumer.Consumer.Shutdown += (sender, shutdownEvent) => {

                    RestablishConnection(consumer, shutdownEvent);
                };
            }
        }

        // Deserialize the message into the type associated with the message dispatch
        // metadata and delegates to the messaging module to dispatch the message to
        // consumer handlers.
        private void MessageReceived(MessageConsumer messageConsumer, BasicDeliverEventArgs deliveryEvent)
        {
            IMessage message = DeserializeMessage(messageConsumer.DispatchInfo.MessageType, deliveryEvent);
            message.SetAcknowledged(false);

            LogReceivedExchangeMessage(message, messageConsumer);

            // Delegate to the Messaging Module to dispatch the message to all consumers.
            Task<IMessage> futureResult = _messagingModule.DispatchConsumer<IMessage>(
                message,
                messageConsumer.DispatchInfo);

            futureResult.Wait();

            if (!messageConsumer.QueueSettings.IsNoAck)
            {
                HandleAcknowledgeMessage(message, messageConsumer, deliveryEvent);
            }

            HandleRejectedMessage(message, messageConsumer, deliveryEvent);
        }

        private void HandleAcknowledgeMessage(IMessage message, MessageConsumer messageConsumer,
            BasicDeliverEventArgs deliveryEvent)
        {
            if (message.GetAcknowledged())
            {
                messageConsumer.Channel.BasicAck(deliveryEvent.DeliveryTag, false);
            }
        }

        private void HandleRejectedMessage(IMessage message, MessageConsumer messageConsumer,
            BasicDeliverEventArgs deliveryEvent)
        {
            if (message.GetRejected())
            {
                messageConsumer.Channel.BasicReject(deliveryEvent.DeliveryTag,
                    message.GetRequeueOnRejection());
            }
        }

        // Allows the running application to publish messages to the defined
        // exchanges to which other consumer processes can receive.
        public async Task PublishToExchange(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            Type messageType = message.GetType();
            IEnumerable<ExchangeDefinition> exchangeDefs = _messageExchanges[messageType];

            if (exchangeDefs == null)
            {
                throw new InvalidOperationException(
                    $"The message of type: {messageType.FullName} is not associated with an exchange.");
            }

            LogPublishingExchangeMessage(message, exchangeDefs);

            foreach(ExchangeDefinition exchangeDef in exchangeDefs)
            {
                await Publish(exchangeDef, message);
            }
        }

        // Publishes a message to a consumer defined queue used for receiving
        // RPC style messages.  The running application awaits the response.
        public async Task PublishToRpcConsumer(IMessage message)
        {
            if (! IsRpcCommand(message))
            {
                throw new InvalidOperationException(
                    $"The message of type: {message.GetType()} is not a command " +
                    $"or is not decorated with: {typeof(RpcCommandAttribute)}.");
            }

            var consumerAtrib = message.GetAttribute<RpcCommandAttribute>();
            var command = message as ICommand;

            RpcMessageConsumer consumer = GetRpcConsumer(consumerAtrib);
            byte[] messageBody = SerializeMessage(command, consumer.DefaultContentType);

            // Publish the RPC request the consumer's queue and await a response.
            byte[] replyBody = await consumer.Client.Invoke(command, messageBody);
   
            object reply = DeserializeReply(consumer.DefaultContentType, command.ResultType, replyBody);
            command.SetResult(reply);
        }

        public bool IsRpcCommand(IMessage message)
        {
            Type messageType = message.GetType();

            return messageType.IsDerivedFrom<ICommand>() 
                && messageType.HasAttribute<RpcCommandAttribute>();
        }

        private RpcMessageConsumer GetRpcConsumer(RpcCommandAttribute consumerAttrib)
        {
            RpcMessageConsumer consumer = _rpcMessageConsumers.FirstOrDefault(c => 
                c.BrokerName == consumerAttrib.BrokerName 
                && c.RequestQueueKey == consumerAttrib.RequestQueueKey);

            if (consumer == null)
            {
                throw new InvalidOperationException(
                    $"RPC Consumer Client could not configured for Broker: {consumerAttrib.BrokerName} " +
                    $"RequestQuoteKey: {consumerAttrib.RequestQueueKey}.");
            }
            return consumer;
        }

        private async Task Publish(ExchangeDefinition exchangeDef, IMessage message)
        {
            if (! await MatchesExchangeCriteria(exchangeDef, message)) return;

            string contentType = exchangeDef.Exchange.Settings.ContentType;
            byte[] messageBody = SerializeMessage(message, contentType);

            using (var channel = CreateBrokerChannel(exchangeDef.Exchange.BrokerName))
            {
                exchangeDef.Exchange.Publish(channel, message, messageBody); 
            }
        }

        // Determines if the message should be delivered to the queue.  If the exchange is marked
        // with a predicate attribute, the corresponding externally named script is executed to 
        // determine if the message has matching criteria.  If no external script is specified,
        // the exchange's matches method is called.
        private async Task<bool> MatchesExchangeCriteria(ExchangeDefinition exchangeDef, IMessage message)
        {
            ScriptPredicate predicate = exchangeDef.Exchange.Settings.Predicate;
            
            if (predicate != null)
            {
                return await _scriptingSrv.SatifiesPredicate(message, predicate);
            }

            return exchangeDef.Exchange.Matches(message);
        }

        private byte[] SerializeMessage(object message, string contentType)
        {
            IBrokerSerializer serializer = GetMessageSerializer(contentType);
            return serializer.Serialize(message);
        }

        private IBrokerSerializer GetMessageSerializer(string contentType)
        {
            IBrokerSerializer serializer = null;

            if (! _brokerConfig.Serializers.TryGetValue(contentType, out serializer))
            {
                _logger.Error($"Serializer for Content Type: {contentType} has not been configured.");
            }

            return serializer;
        }

        private IMessage DeserializeMessage(Type messageType,
            BasicDeliverEventArgs deliveryEvent)
        {
            string contentType = deliveryEvent.BasicProperties.ContentType;

            if (contentType.IsNullOrWhiteSpace())
            {
                _logger.Error(
                    $"the content type of a message corresponding to the message " +
                    $"of type: {messageType} was not specified as a basic property");
            }

            IBrokerSerializer serializer = GetMessageSerializer(contentType);
            return serializer.Deserialize<IMessage>(deliveryEvent.Body, messageType);
        }

        // get from returned basic props
        private object DeserializeReply(string contentType, Type replyType, byte[] replyBody)
        {
            IBrokerSerializer serializer = GetMessageSerializer(contentType);
            return serializer.Deserialize(replyBody, replyType);
        }

        // Called when a channel is notified of an connection issue.  Once the connection is
        // reestablished, the centrally saved exchanges and queues need to be recreated on the
        // new connection.  This is important when using a broker failover behind a load-balancer
        // since the backup broker my not have the exchanges.
        private void RestablishConnection(MessageConsumer messageConsumer, ShutdownEventArgs shutdownEvent)
        {
            BrokerConnection brokerConn = _brokerConfig.Connections[messageConsumer.BrokerName];
            if (brokerConn.Connection.IsOpen) return;

            if (IsUnexpectedShutdown(shutdownEvent))
            {
                _logger.Error("Connection to broker was shutdown.  Reconnection will be attempted.");

                string brokerName = messageConsumer.BrokerName;
                IEnumerable<BrokerMeta> brokerMeta = _exchangeRep.LoadAsync(brokerName).Result;

                ReconnectToBroker(brokerName);

                // Restore the exchanges and queues on what might be a new broker.
                using (IModel channel = CreateBrokerChannel(brokerName))
                {
                    ReCreatePublisherExchanges(channel, brokerMeta);
                    ReCreateConsumerQueues(channel, brokerMeta);
                    BindConsumersToQueues(messageConsumer.BrokerName);
                }

                // Watch for future issues.
                AttachBokerMonitoringHandlers();

                _logger.Debug("Connection to broker was reestablished.");
            }
        }

        private static bool IsUnexpectedShutdown(ShutdownEventArgs shutdownEvent)
        {
            return shutdownEvent.Initiator == ShutdownInitiator.Library 
                || shutdownEvent.Initiator == ShutdownInitiator.Peer;
        }

        private void ReconnectToBroker(string brokerName)
        {
            BrokerConnection brokerConn = _brokerConfig.Connections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new InvalidOperationException(
                   $"An existing broker with the name of: {brokerConn} does not exist.");
            }

            ConnectToBroker(brokerConn);
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
                .Where(e => e.Settings == null)
                .ToList();

            ReCreateExchanges(channel, exchanges);

            _logger.Debug("Consumer queues recreated after broker connection failure.");
        }

        private void ReCreateExchanges(IModel channel, IEnumerable<ExchangeMeta> exchanges)
        {
            foreach (ExchangeMeta exchange in exchanges)
            {
                // If not the default exchange and not consumer reference to exchange:
                if (exchange.Settings != null && exchange.Settings.ExchangeType != null && exchange.Settings != null)
                {
                    channel.ExchangeDeclare(exchange.Settings);
                }

                foreach (var queue in exchange.QueueMeta)
                {
                    channel.QueueDeclare(queue.QueueName, queue.Settings);
                }
            }
        }

        private void LogPublishingExchangeMessage(IMessage message, IEnumerable<ExchangeDefinition> exchanges)
        {
            _logger.Verbose("Published to Exchange", 
                new
                {
                    Message = message,
                    Exchanges = exchanges.Select(e => new {
                        BrokerName = e.Exchange.BrokerName,
                        Exchange = e.Exchange.ExchangeName
                    })
                });
        }

        private void LogReceivedExchangeMessage(IMessage message, MessageConsumer messageConsumer)
        {
            _logger.Verbose("Exchanged Message Received",
                new
                {
                    messageConsumer.BrokerName,
                    messageConsumer.ExchangeName,
                    messageConsumer.RouteKeys,
                    messageConsumer.DispatchInfo.ConsumerType,
                    messageConsumer.DispatchInfo.MessageType,

                    MethodName = messageConsumer.DispatchInfo.MessageHandlerMethod.Name,
                    Message = message,
                });
        }
    }
}
