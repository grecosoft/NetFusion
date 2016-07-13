using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Exchanges;
using NetFusion.RabbitMQ.Integration;
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
    internal class MessageBroker: IDisposable,
        IMessageBroker
    {
        private bool _disposed;

        private readonly BrokerSettings _brokerSettings;
        private readonly IDictionary<string, BrokerConnection> _brokerConnections;
        private readonly ILookup<Type, ExchangeDefinition> _messageExchanges;
        private readonly IDictionary<string, IMessageSerializer> _serializers;

        private IEnumerable<MessageConsumer> _messageConsumers;
        private Func<string, Task<IEnumerable<ExchangeConfig>>> _exchangeMetadataReader;

        /// <summary>
        /// Initializes new broker.
        /// </summary>
        /// <param name="brokerSettings">Broker specific settings.</param>
        /// <param name="brokerConnections">Connection settings for the required
        /// broker connections.</param>
        /// <param name="exchanges">Information for the exchanges and optional queues
        /// to be declared on brokers.</param>
        public MessageBroker(
            BrokerSettings brokerSettings,
            IDictionary<string, BrokerConnection> brokerConnections,
            IEnumerable<IMessageExchange> exchanges)
        {
            Check.NotNull(brokerConnections, nameof(brokerConnections));
            Check.NotNull(exchanges, nameof(exchanges));

            _brokerSettings = brokerSettings;
            _brokerConnections = brokerConnections;
            _serializers = new Dictionary<string, IMessageSerializer>();

            // Messages can have one or more associated exchanges.
            _messageExchanges = exchanges.ToLookup(
                k => k.MessageType,
                e => new ExchangeDefinition(e.MessageType, e));

            AddDefaultSerializers();
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
            _brokerConnections.ForEachValue(bc => bc.Connection.Dispose());
        }

        private void AddDefaultSerializers()
        {
            AddSerializer(new JsonEventMessageSerializer());
            AddSerializer(new BinaryMessageSerializer());
        }

        /// <summary>
        /// Add serialize used to serialize and deserialize messages.  
        /// If there is already a registered content type, it will be replaced.
        /// </summary>
        /// <param name="serializer">The custom serialize to use for messages
        /// with the same specified content-type.</param>
        public void AddSerializer(IMessageSerializer serializer)
        {
            Check.NotNull(serializer, nameof(serializer));

            _serializers[serializer.ContentType] = serializer;
        }

        /// <summary>
        /// Used to specify a delegate that should be called to load cached exchange meta-data.
        /// </summary>
        /// <param name="reader">Delegate that is called with the broker name.</param>
        public void SetExchangeMetadataReader(Func<string, Task<IEnumerable<ExchangeConfig>>> reader)
        {
            Check.NotNull(reader, nameof(reader));

            _exchangeMetadataReader = reader;
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

                exDef.Exchange.InitializeSettings();
                _brokerSettings.ApplyQueueSettings(exDef.Exchange);
                
                using (var channel = CreateBrokerChannel(exDef.Exchange.BrokerName))
                {
                    exDef.Exchange.Declare(channel);
                }
            });
        }

        private void EstablishBrockerConnections()
        {
            foreach(var brokerConn in _brokerConnections.Values)
            {
                ConnectToBroker(brokerConn);
            }
        }

        private static void ConnectToBroker(BrokerConnection brokerConn)
        {
            var connFactory = new ConnectionFactory
            {
                HostName = brokerConn.HostName,
                VirtualHost = brokerConn.VHostName
            };

            brokerConn.Connection = connFactory.CreateConnection();
        }

        private void ReconnectToBroker(string brokerName)
        {
            var brokerConn = _brokerConnections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new InvalidOperationException(
                   $"An existing broker with the name of: {brokerConn} does not exist.");
            }
            ConnectToBroker(brokerConn);
        }

        private IModel CreateBrokerChannel(string brokerName)
        {
            BrokerConnection brokerConn = _brokerConnections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new InvalidOperationException(
                    $"Channel could not be created.  A broker with the name of: {brokerName} does not exist.");
            }

            return brokerConn.Connection.CreateModel();
        }

        /// <summary>
        /// Binds consumers to an existing queue on an exchange or creates a new
        /// queue that is bound to the exchange.
        /// </summary>
        /// <param name="messageConsumers">List configured message consumers.</param>
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
                    _brokerSettings.ApplyQueueSettings(messageConsumer);
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

            // Delegate to the Messaging Module to dispatch the message to all consumers.
            var futureResult = MessagingModule.DispatchConsumer(
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
 
        private static void HandleAcknowledgeMessage(IMessage message, MessageConsumer messageConsumer,
            BasicDeliverEventArgs deliveryEvent)
        {
            if (message.GetAcknowledged())
            {
                messageConsumer.Channel.BasicAck(deliveryEvent.DeliveryTag, false);
            }
        }

        private static void HandleRejectedMessage(IMessage message, MessageConsumer messageConsumer,
            BasicDeliverEventArgs deliveryEvent)
        {
            if (message.GetRejected())
            {
                messageConsumer.Channel.BasicReject(deliveryEvent.DeliveryTag,
                    message.GetRequeueOnRejection());
            }
        }

        /// <summary>
        /// Publishes a message to all associated exchanges
        /// </summary>
        /// <exception cref="InvalidOperationException">Raised if the message
        /// is not associated with one or more exchanges.</exception>
        /// <param name="message">The message to publish to one or more
        /// exchanges.</param>
        public void PublishToExchange(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            var messageType = message.GetType();
            var exchangeDefs = _messageExchanges[messageType];

            if (exchangeDefs == null)
            {
                throw new InvalidOperationException(
                    $"The message of type: {message.GetType().FullName} is not associated with an exchange.");
            }

            exchangeDefs.ForEach(exchangeDef => Publish(exchangeDef, message));
        }

        private void Publish(ExchangeDefinition exchangeDef, IMessage message)
        {
            // Check to see if the message passes the criteria required to be
            // published to the exchange.
            if (!exchangeDef.Exchange.Matches(message)) return;

            var contentType = exchangeDef.Exchange.Settings.ContentType;
            var messageBody = SerializeEvent(message, contentType);
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

        private byte[] SerializeEvent(IMessage message, string contentType)
        {
            var serializer = GetMessageSerializer(contentType);
            return serializer.Serialize(message);
        }

        private IMessage DeserializeMessage(Type messageType,
            BasicDeliverEventArgs deliveryEvent)
        {
            var contentType = deliveryEvent.BasicProperties.ContentType;

            if (contentType.IsNullOrWhiteSpace())
            {
                Plugin.Log.Error(
                    $"the content type of a message corresponding to the message " +
                    $"of type: {messageType} was not specified as a basic property");
            }

            var serializer = GetMessageSerializer(contentType);
            return serializer.Deserialize(deliveryEvent.Body, messageType);
        }

        private IMessageSerializer GetMessageSerializer(string contentType)
        {
            IMessageSerializer serializer = null;

            if (!_serializers.TryGetValue(contentType, out serializer))
            {
                Plugin.Log.Error($"Serializer for Content Type: {contentType} has not been configured.");
            }

            return serializer;
        }

        private void RestablishConnection(object sender, MessageConsumer messageConsumer,
            ShutdownEventArgs shutdownEvent)
        {
            var brokerConn = _brokerConnections[messageConsumer.BrokerName];

            if (brokerConn.Connection.IsOpen) return;

            if (IsUnexpectedShutdown(shutdownEvent))
            {
                ReconnectToBroker(messageConsumer.BrokerName);
                ReCreateExchanges(messageConsumer.BrokerName);
                BindConsumersToQueues(messageConsumer.BrokerName);
            }
        }

        private static bool IsUnexpectedShutdown(ShutdownEventArgs shutdownEvent)
        {
            return shutdownEvent.Initiator == ShutdownInitiator.Library || shutdownEvent.Initiator == ShutdownInitiator.Peer;
        }

        private void ReCreateExchanges(string brokerName)
        {
            var cachedExchanges = _exchangeMetadataReader(brokerName).Result;
            using (var channel = CreateBrokerChannel(brokerName))
            {
                foreach (var exchange in cachedExchanges)
                {
                    ReCreateExchange(channel, exchange);
                }
            }
        }

        private static void ReCreateExchange(IModel channel, ExchangeConfig exchange)
        {
            if (exchange.Settings.ExchangeType != null)
            {
                channel.ExchangeDeclare(exchange.Settings);
            }

            foreach (var queue in exchange.QueueConfigs)
            {
                channel.QueueDeclare(queue.QueueName, queue.Settings);
            }
        }
    }
}
