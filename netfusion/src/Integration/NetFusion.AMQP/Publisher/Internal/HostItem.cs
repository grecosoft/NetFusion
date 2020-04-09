using System;
using System.Text;
using Amqp;
using Amqp.Framing;
using NetFusion.Base;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.AMQP.Publisher.Internal
{
    /// <summary>
    /// Base class representing an item (i.e. Queue/Topic) defined on an host
    /// and can have messages sent.
    /// </summary>
    /// <typeparam name="TMessage">The message type associated with the item.</typeparam>
    public abstract class HostItem<TMessage> : IHostItem,
        ISenderHostItem
        where TMessage : IMessage
    {
        /// <summary>
        /// The host on which the item is defined.
        /// </summary>
        public string HostName { get; }
        
        /// <summary>
        /// The name of the item defined on the host.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the message associated with the host item.
        /// </summary>
        public Type MessageType => typeof(TMessage);
      
        protected HostItem(string hostName, string name)
        {
            HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string ContentType { get; private set; } = ContentTypes.Json;
        public string ContentEncoding { get; private set; }

        public void UseContentType(string contentType, string contentEncoding = null)
        {
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentException("Content Type not specified.", nameof(contentType));
            
            ContentType = contentType;
            ContentEncoding = contentEncoding;
        }

        /// <summary>
        /// Invoked to allow a specific host item definition to set its related message properties.
        /// </summary>
        /// <param name="message">The AMQP message being sent.</param>
        /// <param name="qmqpMessage"></param>
        internal void SetMessageProperties(IMessage message, Message qmqpMessage)
        {
            qmqpMessage.Properties = new Properties
            {
                ContentType = ContentType,
                ContentEncoding = ContentEncoding,
                CreationTime = DateTime.UtcNow
            };
            
            SetPropsFromMessageAttributes(message, qmqpMessage);
        }

        // ISenderHostItem internal interface implementation:
        
        // The AMQP link used to send messages to a host item.
        public ISenderLink SenderLink { get; set; }

        // Creates the AMQP message and allows the derived class to set its
        // message specific properties.
        public Message CreateMessage(IMessage message, object body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            
            var amqpMessage = new Message(body);
            
            SetMessageProperties(message, amqpMessage);
            return amqpMessage;
        }

        // Called to determine if the messaging being published meets the
        // criteria of the defined host item.
        public virtual bool MessageApplies(IMessage message)
        {
            return true;
        }

        // A message in NetFusion has a dynamic set of properties.  The following tests for a set of known
        // named values scoped to the AmqpPropContext class.  If found, the value is set on the AMQP message.
        private static void SetPropsFromMessageAttributes(IMessage message, Message nsMessage)
        {
            nsMessage.Properties.CorrelationId = message.GetCorrelationId();
            nsMessage.Properties.MessageId = message.GetMessageId();
            nsMessage.Properties.Subject = message.GetSubject();
            nsMessage.Properties.To = message.GetTo();
            nsMessage.Properties.ReplyTo = message.GetReplyTo();

            var userId = message.GetUserId();
            if (userId != null)
            {
                nsMessage.Properties.UserId = Encoding.UTF8.GetBytes(userId);
            }

            var absExpiryTime = message.GetAbsoluteExpiryTime();
            if (absExpiryTime != null)
            {
                nsMessage.Properties.AbsoluteExpiryTime = absExpiryTime.Value;
            }

            var ttl = message.GetTls();
            if (ttl != null)
            {
                nsMessage.Header.Ttl = ttl.Value;
            }
        }
    }
}