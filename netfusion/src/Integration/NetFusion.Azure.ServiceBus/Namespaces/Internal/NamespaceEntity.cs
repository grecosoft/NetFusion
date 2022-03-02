using System;
using Azure.Messaging.ServiceBus;
using NetFusion.Base;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Namespaces.Internal
{
    /// <summary>
    /// Represents an entity created within an Azure Service Bus Namespace.
    /// Within Service Bus, an entity can be either a Queue or Topic.
    /// </summary>
    public abstract class NamespaceEntity
    {
        /// <summary>
        /// The namespace as specified within the host's configuration file
        /// where the entity should be created.
        /// </summary>
        public string NamespaceName { get; }

        /// <summary>
        /// The name of the entity contained within the namespace.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// The type of the message associated with the namespace entity.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// The content-type the message should be serialized to when delivered to the entity.
        /// </summary>
        public string ContentType { get; private set; } = ContentTypes.Json;

        protected NamespaceEntity(string namespaceName, string entityName, Type messageType)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace Name must be specified.", nameof(namespaceName));

            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("Entity Name must be specified.", nameof(entityName));

            NamespaceName = namespaceName;
            EntityName = entityName;
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        }

        /// <summary>
        /// Specifies the content-type the message should be serialized to when delivered to the entity.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        public void UseContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type must be specified.", nameof(contentType));

            ContentType = contentType;
        }

        /// <summary>
        /// Indicates that the Service Bus entity will not have messages directly delivered.
        /// This is the case for queues that are sent messages forwarded from other queues.
        /// The forwarding happens directly on the Azure Service Bus.
        /// </summary>
        internal bool IsSecondaryQueue { get; set; }

        /// <summary>
        /// Strategy specific to the Entity indicating how it should be created and other
        /// implemented behaviors.
        /// </summary>
        internal IEntityStrategy EntityStrategy { get; set; }

        /// <summary>
        /// Associated Azure Service Bus object used to send messages to the entity:
        /// </summary>
        internal ServiceBusSender EntitySender { get; set; }

        /// <summary>
        /// Called when message is published allowing properties to be specified on the
        /// delivered ServiceBusMessage.
        /// </summary>
        /// <param name="busMessage">The created Service Bus message.</param>
        /// <param name="message">The application specific message from which
        /// the Service Bus message was created.</param>
        internal virtual void SetBusMessageProps(ServiceBusMessage busMessage, IMessage message)
        {
        }

        public override string ToString() => $"{NamespaceName}:{EntityName}[{MessageType}]";
    }
}