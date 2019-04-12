using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using NetFusion.AMQP.Modules;
using NetFusion.AMQP.Publisher.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;

namespace NetFusion.AMQP.Publisher
{
    /// <summary>
    /// Message publisher that determines if the type of the message being published
    /// has an associated host item (Queue or Topic).
    /// </summary>
    public class HostMessagePublisher : MessagePublisher
    {
        private readonly IConnectionModule _connectionModule;
        private readonly IPublisherModule _publisherModule;
        private readonly ISerializationManager _serialization;
        
        public HostMessagePublisher(
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
        /// associated with the message being published.  If a registration is for an
        /// associated Queue or Topic, the message will be delivered.
        /// </summary>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            Type messageType = message.GetType();
            
            if (! _publisherModule.HasHostItem(messageType))
            {
                return;
            }

            // Get information associated with the message being published:
            IHostItem hostItem = _publisherModule.GetHostItem(messageType);
            ISenderHostItem senderItem = (ISenderHostItem)hostItem;

            Session session = await _connectionModule.CreateSenderSession(hostItem.HostName);
            SenderLink senderLink = null;
            try
            {
                // Create the sender link used to send messages:
                senderLink = new SenderLink(session, Guid.NewGuid().ToString(), senderItem.Name);
            
                // Serialize the message based on its associated content-type and delegate to the corresponding
                // host sender item to create the AMQP message with the message properties correctly set.
                object body = SerializeMessage(hostItem, message);
                Message amqpMessage = senderItem.CreateMessage(message, body);
            
                // Send message to the defined host item (i.e. Queue/Topic).
                await senderLink.SendAsync(amqpMessage);
            }
            finally
            {
                await session.CloseAsync();
                if (senderLink != null)
                {
                    await senderLink.CloseAsync();
                }
            }
        }

        public object SerializeMessage(IHostItem hostItem, IMessage message)
        {
            return _serialization.Serialize(message, hostItem.ContentType, hostItem.ContentEncoding);
        }
    }
}