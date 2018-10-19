using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using NetFusion.Azure.Messaging.Modules;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Publisher
{
    /// <summary>
    /// Message publisher that determines if the type of the message being published
    /// has an associated Queue or Topic.
    /// </summary>
    public class NamespaceMessagePublisher : MessagePublisher
    {
        private readonly IConnectionModule _connectionModule;
        private readonly IPublisherModule _publisherModule;
        private readonly ISerializationManager _serialization;
        
        public NamespaceMessagePublisher(
            IConnectionModule connectionModule,
            IPublisherModule publisherModule,
            ISerializationManager serialization)
        {
            _connectionModule = connectionModule ?? throw new ArgumentNullException(nameof(connectionModule));
            _publisherModule = publisherModule ?? throw new ArgumentNullException(nameof(publisherModule));
            _serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
        }
        
        /// <summary>
        /// Indicates that this message publisher will deliver the message to
        /// an external process.  Not directly used by this plug-in but allows
        /// higher level implementations to, for example, dispatch all in-process
        /// messages before dispatching those that are external.
        /// </summary>
        public override IntegrationTypes IntegrationType => IntegrationTypes.External;

        /// <summary>
        /// Called when a message is published.  Determines if there is a registration
        /// associated with the message being plublished.  If a registration if for an
        /// assocated Queue or Topic, the message will be delivered.
        /// </summary>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            Type messageType = message.GetType();
            
            if (! _publisherModule.HasNamespaceItem(messageType))
            {
                return;
            }

            // Get information associated with the message being published:
            INamespaceItem nsItem = _publisherModule.GetNamespaceItem(messageType);
            ILinkedItem linkedItem = (ILinkedItem)nsItem;
            Session nsSession = await _connectionModule.GetSession(nsItem.Namespace);
            
            // Assure the sender link has been set used to send messages:
            _connectionModule.SetSenderLink(nsSession, linkedItem);

            // Serialize the message based on its assocated conent-type and delegate to the corresponding
            // namespace item to create the AMQP message with the message properties correctly set.
            object body = SerializeMessage(nsItem, message);
            Message nsMessage = linkedItem.CreateMessage(message, body);
            
            // Send message to the defined namespace item (i.e. Queue/Topic).
            await linkedItem.SenderLink.SendAsync(nsMessage);
        }

        public object SerializeMessage(INamespaceItem nsItem, IMessage message)
        {
            return _serialization.Serialize(message, nsItem.ContentType);
        }
    }
}