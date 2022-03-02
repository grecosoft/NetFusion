using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Plugin;
using NetFusion.Base;
using NetFusion.Base.Serialization;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher.Internal
{
    /// <summary>
    /// Message publisher responsible for sending messages to associated Azure Service Bus entities.
    /// This class implements the IMessagePublisher interface and is registered within the central
    /// messaging dispatch pipeline.
    /// </summary>
    public class ServiceBusPublisher : IMessagePublisher
    {
        private readonly ILogger _logger;
        private readonly IPublisherModule _publisherModule;
        private readonly ISerializationManager _serialization;
        
        public ServiceBusPublisher(
            ILogger<ServiceBusPublisher> logger,
            IPublisherModule publisherModule,
            ISerializationManager serialization)
        {
            _logger = logger;
            _publisherModule = publisherModule;
            _serialization = serialization;
        }
        
        // Indicates that this publisher dispatches to external message handlers
        // contained within another running process.
        public IntegrationTypes IntegrationType => IntegrationTypes.External;

        public async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            var messageType = message.GetType();
            
            if (! _publisherModule.TryGetMessageEntity(messageType, out NamespaceEntity entity))
            {
                // No Azure Service Bus associated entity to publish message.
                _logger.LogDebug("No Service Bus namespace entity for message {MessageType}", messageType);
                return;
            }
            
            if (entity is IMessageFilter filter && !filter.Applies(message))
            {
                // Message does not apply to entity.
                _logger.LogDebug("Message {MessageType} does not pass pre-filter for entity {EntityName}",
                    messageType, entity);
                return;
            }

            // Serialize the message and use the namespace entity's associated
            // sender to publish message to the service bus.
            BinaryData messageData = SerializeMessage(entity, message);
            ServiceBusMessage busMessage = new ServiceBusMessage(messageData);

            SetBusMessageProps(busMessage, entity, message);
            
            _logger.LogDebug("Message {MessageType} being sent to entity {EntityName}", messageType, entity);

            // Determine if the entity's strategy supports custom publishing logic.
            if (entity.EntityStrategy is IPublishStrategy supported)
            {
                await supported.SendToEntityAsync(message, busMessage, cancellationToken);
                return;
            }
            
            // Otherwise, send the message directly to the service-bus.
            await entity.EntitySender.SendMessageAsync(busMessage, cancellationToken);
        }
        
        private BinaryData SerializeMessage(NamespaceEntity entity, IMessage message)
        {
            byte[] messageData = _serialization.Serialize(message, entity.ContentType);
            return new BinaryData(messageData);
        }
        
        // Allows the entity associated with the method to specify any out going message properties.
        // Then any properties specified directly on the message can override these values.
        private static void SetBusMessageProps(ServiceBusMessage busMessage, NamespaceEntity entity, IMessage message)
        {
            entity.SetBusMessageProps(busMessage, message);
            
            SetPropsFromEntity(busMessage, entity);
            SetIdentityPropsFromMessage(busMessage, message);
            SetTimeBasedPropsFromMessage(busMessage, message);
            SetDescriptivePropsFromMessage(busMessage, message);
        }

        private static void SetPropsFromEntity(ServiceBusMessage busMessage, NamespaceEntity entity)
        {
            busMessage.ContentType ??= entity.ContentType ?? ContentTypes.Json;
        }

        private static void SetIdentityPropsFromMessage(ServiceBusMessage busMessage, IMessage message)
        {   
            // Since a MessageId is required for RPC style messages, one is automatically generated
            // if not yet specified by the time the message is published.
            busMessage.MessageId = message.GetMessageId() ?? Guid.NewGuid().ToString();

            string correlationId = message.GetCorrelationId();
            if (correlationId != null)
            {
                busMessage.CorrelationId = correlationId;
            }
        }

        private static void SetTimeBasedPropsFromMessage(ServiceBusMessage busMessage, IMessage message)
        {
            TimeSpan? timeToLive = message.GetTimeToLive();
            if (timeToLive != null)
            {
                busMessage.TimeToLive = timeToLive.Value;
            }
        }

        private static void SetDescriptivePropsFromMessage(ServiceBusMessage busMessage, IMessage message)
        {
            string subject = message.GetSubject();
            if (subject != null)
            {
                busMessage.Subject = subject;
            }
        }
    }
}