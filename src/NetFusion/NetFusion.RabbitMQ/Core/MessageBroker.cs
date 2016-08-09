using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
    /// specific to consumers.
    /// </summary>
    public class MessageBroker: IDisposable,
        IMessageBroker
    {
        private bool _disposed;
        private readonly IContainerLogger _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly IEntityScriptingService _scriptingSrv;

        private MessageBrokerConfig _brokerConfig;
        private ILookup<Type, ExchangeDefinition> _messageExchanges;
        private IEnumerable<MessageConsumer> _messageConsumers;

        public MessageBroker(IContainerLogger logger, 
            IMessagingModule messagingModule,
            IEntityScriptingService scriptingSrv)
        {
            _logger = logger.ForContext<MessageBroker>();
            _messagingModule = messagingModule;
            _scriptingSrv = scriptingSrv;
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
                e => new ExchangeDefinition(e.MessageType, e));
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

            return _messageExchanges[message.GetType()] != null;
        }

        /// <summary>
        /// Creates all of the exchanges to which messages can be published
        /// as serialized messages.  Any default queue configurations specified
        /// by the exchange will also be created.
        /// </summary>
        public void DefineExchanges()
        {
            EstablishBrockerConnections();

            _messageExchanges.ForEachValue(exDef => {

                using (var channel = CreateBrokerChannel(exDef.Exchange.BrokerName))
                {
                    exDef.Exchange.Declare(channel);
                }
            });
        }

        private void EstablishBrockerConnections()
        {
            foreach(var brokerConn in _brokerConfig.Connections.Values)
            {
                ConnectToBroker(brokerConn);
            }
        }

        protected virtual void ConnectToBroker(BrokerConnection brokerConn)
        {
            var connFactory = new ConnectionFactory
            {
                HostName = brokerConn.HostName,
                VirtualHost = brokerConn.VHostName
            };

            brokerConn.Connection = connFactory.CreateConnection();
        }

        private IModel CreateBrokerChannel(string brokerName)
        {
            BrokerConnection brokerConn = _brokerConfig.Connections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new InvalidOperationException(
                    $"Channel could not be created.  A broker with the name of: {brokerName} does not exist.");
            }

            return brokerConn.Connection.CreateModel();
        }

        public void BindConsumers(IEnumerable<MessageConsumer> messageConsumers)
        {
            Check.NotNull(messageConsumers, nameof(messageConsumers));

            _messageConsumers = messageConsumers;
            BindConsumersToQueues();
        }

        private void BindConsumersToQueues(string brokerName = null)
        {
            var messageConsumers = brokerName == null ? _messageConsumers :
                _messageConsumers.Where(c => c.BrokerName == brokerName);

            foreach (var messageConsumer in messageConsumers)
            {
                CreateConsumerQueue(messageConsumer);

                var consumerChannel = CreateBrokerChannel(messageConsumer.BrokerName);
                messageConsumer.Channel = consumerChannel;

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

        private void CreateConsumerQueue(MessageConsumer eventConsumer)
        {
            if (eventConsumer.BindingType == QueueBindingTypes.Create)
            {
                using (var channel = CreateBrokerChannel(eventConsumer.BrokerName))
                {
                    channel.QueueDeclare(eventConsumer);
                }
            }
        }

        private void AttachConsumerHandlers(MessageConsumer messageConsumer)
        {
            messageConsumer.Consumer.Received += (sender, deliveryEvent) => {

                MessageReceived(messageConsumer, deliveryEvent);
            };

            messageConsumer.Consumer.Shutdown += (sender, shutdownEvent) => {

                RestablishConnection(sender, messageConsumer, shutdownEvent);
            };
        }

        private void MessageReceived(MessageConsumer messageConsumer, BasicDeliverEventArgs deliveryEvent)
        {
            var message = DeserializeMessage(messageConsumer.DispatchInfo.MessageType, deliveryEvent);
            message.SetAcknowledged(false);

            LogReceivedExchangeMessage(message, messageConsumer);

            // Delegate to the Messaging Module to dispatch the message to all consumers.
            var futureResult = _messagingModule.DispatchConsumer(
                message,
                messageConsumer.DispatchInfo);

            futureResult.Wait();
            var response = futureResult.Result;

            if (response != null)
            {
                SendOptionalResponse(messageConsumer, deliveryEvent, response);
            }

            if (!messageConsumer.QueueSettings.IsNoAck)
            {
                HandleAcknowledgeMessage(message, messageConsumer, deliveryEvent);
            }

            HandleRejectedMessage(message, messageConsumer, deliveryEvent);
        }

        // Determine if the message received is configured as a RPC, and if so, publish a message
        // with the results returned from the handling of the message.
        private void SendOptionalResponse(MessageConsumer eventConsumer, BasicDeliverEventArgs deliveryEvent, IMessage message)
        {
            if (deliveryEvent.BasicProperties.ReplyTo != null)
            {
                var contentType = deliveryEvent.BasicProperties.ContentType;
                byte[] messageBody = SerializeEvent(message, contentType);
                var basicProps = GetRpcResponseBasicProperties(eventConsumer, contentType);

                eventConsumer.Channel.BasicPublish(exchange: "",
                    routingKey: deliveryEvent.BasicProperties.ReplyTo,
                    basicProperties: basicProps,
                    body: messageBody);
            }
        }

        private static IBasicProperties GetRpcResponseBasicProperties(MessageConsumer messageConsumer, string contentType)
        {
            var basicProps = messageConsumer.Channel.CreateBasicProperties();

            basicProps.ContentType = contentType;
            return basicProps;
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

        public async Task PublishToExchange(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            var messageType = message.GetType();
            var exchangeDefs = _messageExchanges[messageType];

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

        private async Task Publish(ExchangeDefinition exchangeDef, IMessage message)
        {
            // Check to see if the message passes the criteria required to be
            // published to the exchange.
            if (! await MatchesExchangeCriteria(exchangeDef, message)) return;

            string contentType = exchangeDef.Exchange.Settings.ContentType;
            byte[] messageBody = SerializeEvent(message, contentType);
            ReplyConsumer returnQueueConsumer = null;

            using (var channel = CreateBrokerChannel(exchangeDef.Exchange.BrokerName))
            {
                returnQueueConsumer = CreateReturnQueueConsumer(channel, exchangeDef);
                var replyQueueName = returnQueueConsumer?.ReplyQueueName;

                exchangeDef.Exchange.Publish(channel, message,
                    messageBody,
                    replyQueueName);

                if (returnQueueConsumer != null)
                {
                    HandleOptionalResponse(message, returnQueueConsumer);
                }
            }
        }

        private async Task<bool> MatchesExchangeCriteria(ExchangeDefinition exchangeDef, IMessage message)
        {
            ScriptPredicate predicate = exchangeDef.Exchange.Settings.Predicate;
            
            if (predicate != null)
            {
                return await _scriptingSrv.SatifiesPredicate(message, predicate);
            }

            return exchangeDef.Exchange.Matches(message);
        }

        private byte[] SerializeEvent(IMessage message, string contentType)
        {
            var serializer = GetMessageSerializer(contentType);
            return serializer.Serialize(message);
        }

        private IMessageSerializer GetMessageSerializer(string contentType)
        {
            IMessageSerializer serializer = null;

            if (!_brokerConfig.Serializers.TryGetValue(contentType, out serializer))
            {
                _logger.Error($"Serializer for Content Type: {contentType} has not been configured.");
            }

            return serializer;
        }

        private static ReplyConsumer CreateReturnQueueConsumer(IModel channel, ExchangeDefinition exchangeDef)
        {
            if (exchangeDef.Exchange.ReturnType == null) return null;

            var replyQueueName = channel.QueueDeclare().QueueName;
            var queueConsumer = new QueueingBasicConsumer(channel);

            channel.BasicConsume(replyQueueName, true, queueConsumer);

            return new ReplyConsumer {
                ReplyQueueName = replyQueueName,
                ReturnType = exchangeDef.Exchange.ReturnType,
                Consumer = queueConsumer };
        }

        private void HandleOptionalResponse(IMessage publishedMessage, ReplyConsumer returnQueueConsumer)
        {
            var replyEvent = returnQueueConsumer.Consumer.Queue.Dequeue() as BasicDeliverEventArgs;
            if (replyEvent != null)
            {
                var responseEvent = DeserializeMessage(returnQueueConsumer.ReturnType, replyEvent);
                responseEvent.SetAcknowledged(true);

                (publishedMessage as ICommand)?.SetResult(responseEvent);
            }
        }

        private IMessage DeserializeMessage(Type messageType,
            BasicDeliverEventArgs deliveryEvent)
        {
            var contentType = deliveryEvent.BasicProperties.ContentType;

            if (contentType.IsNullOrWhiteSpace())
            {
                _logger.Error(
                    $"the content type of a message corresponding to the message " +
                    $"of type: {messageType} was not specified as a basic property");
            }

            var serializer = GetMessageSerializer(contentType);
            return serializer.Deserialize(deliveryEvent.Body, messageType);
        }

        // TODO:  Review the following:

        private void RestablishConnection(object sender, MessageConsumer messageConsumer,
            ShutdownEventArgs shutdownEvent)
        {
            var brokerConn = _brokerConfig.Connections[messageConsumer.BrokerName];

            if (brokerConn.Connection.IsOpen) return;

            if (IsUnexpectedShutdown(shutdownEvent))
            {
                ReconnectToBroker(messageConsumer.BrokerName);
                //ReCreateExchanges(messageConsumer.BrokerName);
                BindConsumersToQueues(messageConsumer.BrokerName);
            }
        }

        private static bool IsUnexpectedShutdown(ShutdownEventArgs shutdownEvent)
        {
            return shutdownEvent.Initiator == ShutdownInitiator.Library || shutdownEvent.Initiator == ShutdownInitiator.Peer;
        }

        private void ReconnectToBroker(string brokerName)
        {
            var brokerConn = _brokerConfig.Connections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new InvalidOperationException(
                   $"An existing broker with the name of: {brokerConn} does not exist.");
            }
            ConnectToBroker(brokerConn);
        }

        private void LogPublishingExchangeMessage(IMessage message, IEnumerable<ExchangeDefinition> exchanges)
        {
            _logger.Debug("Published to Exchange", 
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
            _logger.Debug("Exchanged Message Received",
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
