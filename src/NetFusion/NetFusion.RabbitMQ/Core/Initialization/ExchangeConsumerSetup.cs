using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
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
    public class ExchangeConsumerSetup
    {
        private readonly IContainerLogger _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly MessageBrokerConfig _brokerConfig;
        private readonly IConnectionManager _connMgr;
        private readonly ISerializationManager _serializationMgr;

        private IEnumerable<MessageConsumer> _messageConsumers;

        public ExchangeConsumerSetup(
           IContainerLogger logger,
           IMessagingModule messagingModule,
           MessageBrokerConfig brokerConfig,
           IConnectionManager connectionManager,
           ISerializationManager serializationManger)
        {
            _logger = logger.ForPluginContext<ExchangePublisherSetup>();
            _messagingModule = messagingModule;
            _brokerConfig = brokerConfig;
            _connMgr = connectionManager;
            _serializationMgr = serializationManger;
        }

        /// <summary>
        /// Binds consumers to existing queues created by publishers or creates a consumer
        /// specific queue to which it binds.
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

                // Create the channel to listen on for messages.
                IModel consumerChannel = _connMgr.CreateChannel(messageConsumer.BrokerName);
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
            if (consumer.QueueSettings.PrefetchSize != null || consumer.QueueSettings.PrefetchCount != null)
            {
                channel.BasicQos(consumer.QueueSettings.PrefetchSize ?? 0, consumer.QueueSettings.PrefetchCount ?? 0, false);
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
                    // Since an unexpected exception occurred, reject the message and
                    // have the broker re-queue the message for another consumer.
                    this.RejectAndRequeueMessage(messageConsumer, deliveryEvent);
                    _logger.Error("Error Consuming Message", ex);
                }
            };
        }

        // Deserialize the message into the type associated with the message dispatch
        // metadata and delegates to the messaging module to dispatch the message to
        // consumer handlers.
        private void MessageReceived(MessageConsumer messageConsumer, BasicDeliverEventArgs deliveryEvent)
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
    }
}
