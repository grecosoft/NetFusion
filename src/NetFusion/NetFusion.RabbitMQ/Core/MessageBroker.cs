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
    /// then processed when consumers are connected. 
    /// 
    /// This class also implements the process for joining consumers to existing queues 
    /// and for creating new queues specific to a consumer.  When messages are received,
    /// the Messaging Module is delegated to for dispatching the message to the associated
    /// message handler.
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
        private IList<RpcMessagePublisher> _rpcMessagePublishers;
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
            _rpcMessagePublishers = new List<RpcMessagePublisher>();
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

        #region Broker Initialization

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

        /// <summary>
        /// Creates all of the exchanges to which messages can be published.
        /// Any default queue configurations specified by the exchange are also created.
        /// </summary>
        public void ConfigureBroker()
        {
            EstablishBrockerConnections();

            DeclareExchanges();
            CreateRpcMessagePublishers();
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
        private void CreateRpcMessagePublishers(string brokerName = null)
        {
            IEnumerable<BrokerConnection> brokerConnections = _brokerConfig.Settings.Connections;

            if (brokerName != null)
            {
                brokerConnections = brokerConnections.Where(c => c.BrokerName == brokerName);
            }

            foreach (BrokerConnection brokerConn in brokerConnections)
            {
                foreach (RpcConsumerSettings consumer in brokerConn.RpcConsumers)
                {
                    IModel replyChannel = CreateBrokerChannel(brokerConn.BrokerName);
              
                    var rpcClient = new RpcClient(consumer, replyChannel);
                    var rpcPublisher = new RpcMessagePublisher(brokerConn.BrokerName, consumer, rpcClient);

                    AttachRpcBrokerMonitoringHandlers(rpcPublisher);
                    _rpcMessagePublishers.Add(rpcPublisher);
                }
            }
        }

        private void AttachRpcBrokerMonitoringHandlers(RpcMessagePublisher rpcPublisher)
        {
            rpcPublisher.Client.Consumer.Shutdown += (sender, shutdownEvent) => {

                if (IsUnexpectedShutdown(shutdownEvent))
                {
                    ReconnectToBroker(rpcPublisher.BrokerName);
                    RemoveRpcBrokerConsumers(rpcPublisher.BrokerName);
                    CreateRpcMessagePublishers(rpcPublisher.BrokerName);
                }
            };
        }

        private void RemoveRpcBrokerConsumers(string brokerName)
        {
            IEnumerable<RpcMessagePublisher> rpcPublishers = _rpcMessagePublishers
                .Where(c => c.BrokerName == brokerName).ToList();

            foreach (RpcMessagePublisher rpcPublisher in rpcPublishers)
            {
                (rpcPublisher.Client as IDisposable).Dispose();
                _rpcMessagePublishers.Remove(rpcPublisher);
            }
        }

        // Uses the dispatch metadata of the core messaging plug-in and
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

                SetBasicQosProperties(messageConsumer, consumerChannel);

                // Bind to the existing or newly created queue to the exchange
                // if the default exchange is not specified.
                if (!messageConsumer.ExchangeName.IsNullOrWhiteSpace())
                {
                    _brokerConfig.Settings.ApplyQueueSettings(messageConsumer);
                    consumerChannel.QueueBind(messageConsumer);
                }

                consumerChannel.SetBasicConsumer(messageConsumer);
                AttachConsumerHandlers(messageConsumer);
            }
        }

        private void SetBasicQosProperties(MessageConsumer consumer, IModel channel)
        {
            if (consumer.PrefetchSize != null || consumer.PrefetchCount != null)
            {
                channel.BasicQos(consumer.PrefetchSize ?? 0, consumer.PrefetchCount ?? 0, false);
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

                try
                {
                    MessageReceived(messageConsumer, deliveryEvent);
                }
                catch (Exception ex)
                {
                    this.RejectAndRequeueMessage(messageConsumer, deliveryEvent);
                    _logger.Error("Error Consuming Message", ex);
                }
            };
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

        #endregion

        #region Connection Management

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
                throw new BrokerException(
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

        private void DisposeConnections()
        {
            _messageConsumers.ForEach(c => c.Channel?.Dispose());
            _brokerConfig.Connections.ForEachValue(bc => bc.Connection.Dispose());
        }

        #endregion  

        #region Exchange Publishing

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

        // Allows the running application to publish messages to the defined
        // exchanges to which other consumer processes can receive.
        public async Task PublishToExchange(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            Type messageType = message.GetType();
            IEnumerable<ExchangeDefinition> exchangeDefs = _messageExchanges[messageType];

            if (exchangeDefs == null)
            {
                throw new BrokerException(
                    $"The message of type: {messageType.FullName} is not associated with an exchange.");
            }

            foreach (ExchangeDefinition exchangeDef in exchangeDefs)
            {
                await Publish(exchangeDef, message);
            }
        }

        private async Task Publish(ExchangeDefinition exchangeDef, IMessage message)
        {
            if (!await MatchesExchangeCriteria(exchangeDef, message)) return;

            IMessageExchange exchange = exchangeDef.Exchange;

            string contentType = GetFistContentType(
                message.GetContentType(),
                exchange.Settings.ContentType);

            message.SetContentType(contentType);

            LogPublishingExchangeMessage(message, exchangeDef);

            byte[] messageBody = SerializeMessage(message, contentType);

            using (var channel = CreateBrokerChannel(exchange.BrokerName))
            {
                exchangeDef.Exchange.Publish(channel, message, messageBody);
            }
        }

        // Determines if the message should be delivered to the queue.  If the exchange is marked
        // with a predicate attribute, the corresponding externally named script is executed to 
        // determine if the message has passing criteria.  If no external script is specified,
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

        // Deserialize the message into the type associated with the message dispatch
        // metadata and delegates to the messaging module to dispatch the message to
        // consumer handlers.
        private void MessageReceived(MessageConsumer messageConsumer, BasicDeliverEventArgs deliveryEvent)
        {
            IMessage message = DeserializeMessage(messageConsumer.DispatchInfo.MessageType, deliveryEvent);
            message.SetAcknowledged(false);

            LogReceivedExchangeMessage(message, messageConsumer);

            // Delegate to the Messaging Module to dispatch the message to queue consumer.
            Task<object> futureResult = _messagingModule.InvokeDispatcher(
               messageConsumer.DispatchInfo, message);

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

        private void RejectAndRequeueMessage(MessageConsumer messageConsumer, BasicDeliverEventArgs deliveryEvent)
        {
            messageConsumer.Channel.BasicReject(deliveryEvent.DeliveryTag, true);
        }

        #endregion

        #region RPC Publishing

        public bool IsRpcCommand(IMessage message)
        {
            Type messageType = message.GetType();

            return messageType.IsDerivedFrom<ICommand>()
                && messageType.HasAttribute<RpcCommandAttribute>();
        }

        private void AssertRpcCommand(IMessage message)
        {
            if (!IsRpcCommand(message))
            {
                throw new InvalidOperationException(
                    $"The message of type: {message.GetType()} is not a command " +
                    $"or is not decorated with: {typeof(RpcCommandAttribute)}.");
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

        // Publishes a message to a consumer defined queue used for receiving
        // RPC style messages.  The caller awaits the response.
        public async Task PublishToRpcConsumer(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            AssertRpcCommand(message);

            var rpcCommandAttrib = message.GetAttribute<RpcCommandAttribute>();
            var command = message as ICommand;

            RpcProperties rpcProps = rpcCommandAttrib.ToRpcProps();
            RpcMessagePublisher rpcPublisher = GetRpcPublisher(rpcCommandAttrib);

            rpcProps.ContentType = GetFistContentType(
                message.GetContentType(),
                rpcProps.ContentType,
                rpcPublisher.ContentType);

            command.SetContentType(rpcProps.ContentType);

            LogPublishedRpcMessage(message, rpcPublisher, rpcProps);

            // Publish the RPC request the consumer's queue and await a response.
            byte[] messageBody = SerializeMessage(command, rpcProps.ContentType);
            byte[] replyBody = await rpcPublisher.Client.Invoke(command, rpcProps, messageBody);

            object reply = DeserializeReply(rpcProps.ContentType, command.ResultType, replyBody);
            command.SetResult(reply);

            LogReceivedRpcResponse(message, rpcPublisher);
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

                RpcConsumerReplyReceived(channel, deliveryEvent);
            };
        }

        // Publish the reply back to the publisher that made the request.
        private void PublishConsumerReply(object response, IModel channel, IBasicProperties requestProps)
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

        // Called when a RPC style message is received on a queue defined by a 
        // derived RpcExchange class that receives RPC style requests.
        private void RpcConsumerReplyReceived(IModel channel, BasicDeliverEventArgs deleveryEvent)
        {
            ValidateRpcReply(deleveryEvent);

            string typeName = deleveryEvent.BasicProperties.Type;
            Type commandType = _brokerConfig.RpcTypes[typeName];
            MessageDispatchInfo dispatcher = _messagingModule.GetInProcessCommandDispatcher(commandType);
            IMessage message = DeserializeMessage(commandType, deleveryEvent);

            object result = _messagingModule.InvokeDispatcher(dispatcher, message).Result;
            PublishConsumerReply(result, channel, deleveryEvent.BasicProperties);
        }

        private void ValidateRpcReply(BasicDeliverEventArgs deleveryEvent)
        {
            string messageType = deleveryEvent.BasicProperties.Type;

            if (messageType.IsNullOrWhiteSpace())
            {
                throw new BrokerException(
                    "The basic properties of the received RPC request does not specify the message type.");
            }

            if (!_brokerConfig.RpcTypes.ContainsKey(messageType))
            {
                throw new BrokerException(
                    $"The type associated with the message type name: {messageType} could not be resolved.");
            }
        }

        #endregion

        #region Serialization

        private string GetFistContentType(params string[] contentTypes)
        {
            string contentType = contentTypes.FirstOrDefault(ct => ct != null);
            if (contentType == null)
            {
                throw new BrokerException("Serialization type not specified.");
            }
            return contentType;
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
                _logger.Error($"Serializer for Content Type: {contentType} not registered.");
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
                    $"The content type for a message of type: {messageType} was not " + 
                    $"specified as a basic property.");
            }

            IBrokerSerializer serializer = GetMessageSerializer(contentType);
            return serializer.Deserialize<IMessage>(deliveryEvent.Body, messageType);
        }

        private object DeserializeReply(string contentType, Type replyType, byte[] replyBody)
        {
            IBrokerSerializer serializer = GetMessageSerializer(contentType);
            return serializer.Deserialize(replyBody, replyType);
        }

        #endregion

        #region Broker Reconnection

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
                    BindConsumersToRpcQueues();
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
                throw new BrokerException(
                   $"An existing broker with the name of: {brokerConn} does not exist.");
            }

            if (brokerConn.Connection == null || !brokerConn.Connection.IsOpen)
            {
                ConnectToBroker(brokerConn);
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

        #region Logging

        private void LogPublishingExchangeMessage(IMessage message, 
            ExchangeDefinition exchangeDef)
        {
            _logger.Verbose("Publishing to Exchange", () =>
            {
                return new {
                    Message = message,
                    ContentType = message.GetContentType(),
                    exchangeDef.Exchange.BrokerName,
                    exchangeDef.Exchange.ExchangeName
                };
            });
        }

        private void LogReceivedExchangeMessage(IMessage message, 
            MessageConsumer messageConsumer)
        {
            MessageDispatchInfo dispatchInfo = messageConsumer.DispatchInfo;

            _logger.Verbose("Exchange Message Received", () =>
            {
                return new {
                    messageConsumer.BrokerName,
                    messageConsumer.ExchangeName,
                    messageConsumer.RouteKeys,
                    dispatchInfo.ConsumerType,
                    dispatchInfo.MessageType,

                    MethodName = dispatchInfo.MessageHandlerMethod.Name,
                    Message = message
                };
            });      
        }

        private void LogPublishedRpcMessage(IMessage message, 
            RpcMessagePublisher rpcPublisher, 
            RpcProperties rpcProps)
        {
            _logger.Verbose("Publishing to RPC Consumer", () => {
                return new {
                    Message = message,
                    rpcPublisher.BrokerName,
                    rpcPublisher.RequestQueueKey,
                    rpcPublisher.RequestQueueName,
                    rpcPublisher.Client.ReplyQueueName,
                    rpcProps.ContentType,
                    rpcProps.ExternalTypeName
                };
            });
        }

        private void LogReceivedRpcResponse(IMessage message, 
            RpcMessagePublisher rpcPublisher)
        {
            _logger.Verbose("RPC Reply Message Received", () => {
                return new {
                    Message = message,
                    rpcPublisher.Client.ReplyQueueName
                };
            });
        }

        #endregion
    }
}
