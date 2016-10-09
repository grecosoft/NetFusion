using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for subscribing to message queues and 
    /// consuming the published messages.
    /// </summary>
    public class ExchangeConsumerSetup: IDisposable
    {
        private bool _disposed;

        private readonly IContainerLogger _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly MessageBrokerSetup _brokerSetup;
        private readonly IConnectionManager _connMgr;
        private readonly ISerializationManager _serializationMgr;

        private IEnumerable<MessageConsumer> _messageConsumers;

        public ExchangeConsumerSetup(
           IContainerLogger logger,
           IMessagingModule messagingModule,
           MessageBrokerSetup brokerSetup,
           IConnectionManager connectionManager,
           ISerializationManager serializationManger)
        {
            _logger = logger.ForPluginContext<ExchangePublisherSetup>();
            _messagingModule = messagingModule;
            _brokerSetup = brokerSetup;
            _connMgr = connectionManager;
            _serializationMgr = serializationManger;
        }

        /// <summary>
        /// Binds consumers to existing queues created by publishers or creates a consumer
        /// specific queue to which it binds.  By default the number of consumers is 1 but
        /// can be specified within the configuration to allow processing of messages by 
        /// multiple consumer threads.
        /// </summary>
        /// <param name="consumers">The list of consumers defined within the running application.</param>
        /// <param name="brokerName">The optional name of the broker to which consumers should be created.</param>
        public void BindConsumersToQueues(IEnumerable<MessageConsumer> consumers, string brokerName = null)
        {
            Check.NotNull(consumers, nameof(consumers));
                
            _messageConsumers = consumers;

            IEnumerable<MessageConsumer> messageConsumers = brokerName == null ? _messageConsumers :
                _messageConsumers.Where(c => c.BrokerName == brokerName).ToList();

            foreach (MessageConsumer messageConsumer in messageConsumers)
            {
                CreateConsumerQueue(messageConsumer);

                int numberConsumers = GetNumberQueueConsumers(messageConsumer);
                for (var x = 0; x < numberConsumers; x++)
                {
                    AddMessageHandler(messageConsumer);
                }
            }
        }

        // Consuming applications can create queues specifically for them based on the
        // application needs.  
        private void CreateConsumerQueue(MessageConsumer eventConsumer)
        {
            if (eventConsumer.BindingType == QueueBindingTypes.Create)
            {
                using (IModel channel = _connMgr.CreateChannel(eventConsumer.BrokerName))
                {
                    channel.QueueDeclare(eventConsumer);
                }
            }
        }

        private int GetNumberQueueConsumers(MessageConsumer messageConsumer)
        {
            BrokerConnectionSettings conn = _brokerSetup.BrokerSettings.GetConnection(messageConsumer.BrokerName);
            return conn.GetQueueProperties(messageConsumer.QueueName).NumberConsumers;
        }

        private void AddMessageHandler(MessageConsumer messageConsumer)
        {
            // Create the channel to listen on for messages.
            IModel consumerChannel = _connMgr.CreateChannel(messageConsumer.BrokerName);

            SetBasicQosProperties(messageConsumer, consumerChannel);

            // Bind to the existing or newly created queue to the exchange
            // if the default exchange is not specified.
            if (!messageConsumer.ExchangeName.IsNullOrWhiteSpace())
            {
                _brokerSetup.BrokerSettings.ApplyQueueSettings(messageConsumer);
                consumerChannel.QueueBind(messageConsumer);
            }

            EventingBasicConsumer eventConsumer = consumerChannel.GetBasicConsumer(messageConsumer);
            var messageHandler = new MessageHandler(consumerChannel, eventConsumer);

            AttachConsumerHandlers(messageConsumer, messageHandler);
            messageConsumer.MessageHandlers.Add(messageHandler);
        }

        private void SetBasicQosProperties(MessageConsumer consumer, IModel channel)
        {
            channel.BasicQos(consumer.QueueSettings.PrefetchSize ?? 0, consumer.QueueSettings.PrefetchCount ?? 1, false);
        }

        // Process queue messages when they are received.
        private void AttachConsumerHandlers(MessageConsumer messageConsumer, MessageHandler messageHandler)
        {
                messageHandler.EventConsumer.Received += (sender, deliveryEvent) => {

                try
                {
                    MessageReceived(messageConsumer, messageHandler.Channel, deliveryEvent);
                }
                catch (Exception ex)
                {
                    // Since an unexpected exception occurred, reject the message and
                    // have the broker re-queue the message for another consumer.
                    this.RejectAndRequeueMessage(messageHandler.Channel, deliveryEvent);
                    _logger.Error("Error Consuming Message", ex);
                }
            };
        }

        // Deserialize the message into the type associated with the message dispatch
        // metadata and delegates to the messaging module to dispatch the message to
        // consumer handlers.
        private void MessageReceived(MessageConsumer messageConsumer, IModel channel, BasicDeliverEventArgs deliveryEvent)
        {
            IMessage message = _serializationMgr.Deserialize(messageConsumer.DispatchInfo.MessageType, deliveryEvent);
            message.SetAcknowledged(false);

            LogReceivedExchangeMessage(message, messageConsumer);

            // Delegate to the Messaging Module to dispatch the message to queue consumer.
            Task<object> futureResult = _messagingModule.InvokeDispatcherAsync(
               messageConsumer.DispatchInfo, message);

            futureResult.Wait();

            if (!messageConsumer.QueueSettings.IsNoAck)
            {
                HandleAcknowledgeMessage(message, channel, deliveryEvent);
            }

            HandleRejectedMessage(message, channel, deliveryEvent);
        }

        private void HandleAcknowledgeMessage(IMessage message, IModel channel,
            BasicDeliverEventArgs deliveryEvent)
        {
            if (message.GetAcknowledged())
            {
                channel.BasicAck(deliveryEvent.DeliveryTag, false);
            }
        }

        private void HandleRejectedMessage(IMessage message, IModel channel,
            BasicDeliverEventArgs deliveryEvent)
        {
            if (message.GetRejected())
            {
                channel.BasicReject(deliveryEvent.DeliveryTag,
                    message.GetRequeueOnRejection());
            }
        }

        private void RejectAndRequeueMessage(IModel channel, BasicDeliverEventArgs deliveryEvent)
        {
            channel.BasicReject(deliveryEvent.DeliveryTag, true);
        }

        private void LogReceivedExchangeMessage(IMessage message,
            MessageConsumer messageConsumer)
        {
            MessageDispatchInfo dispatchInfo = messageConsumer.DispatchInfo;

            _logger.Verbose("Exchange Message Received", () =>
            {
                return new
                {
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;
            if (_messageConsumers == null) return;

            foreach (MessageHandler handler in _messageConsumers.SelectMany(mc => mc.MessageHandlers))
            {
                handler.Channel?.Dispose();
            }

            _disposed = true;
        }
    }
}
