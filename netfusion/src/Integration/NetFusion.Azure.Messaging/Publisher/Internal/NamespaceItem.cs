using System;
using System.Text;
using Amqp;
using Amqp.Framing;
using NetFusion.Base;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Publisher.Internal
{
    /// <summary>
    /// Base class representing an item (i.e. Queue/Topic) that can be defined
    /// on an Azure namespace.  
    /// </summary>
    /// <typeparam name="TMessage">The message type associated with the item.</typeparam>
    public abstract class NamespaceItem<TMessage> : INamespaceItem,
        ILinkedItem
        where TMessage : IMessage
    {
        /// <summary>
        /// The namespace on which the item is defined.
        /// </summary>
        public string Namespace { get; }
        
        /// <summary>
        /// The name of the object defined on the namespace.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the message associated with the namespace object.
        /// </summary>
        public Type MessageType => typeof(TMessage);
      
        protected NamespaceItem(string namespaceName, string name)
        {
            Namespace = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
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
        /// Invoked to allow a specific namespace item definition to set
        /// its related message properties.
        /// </summary>
        /// <param name="message">The AMQP message being sent.</param>
        /// <param name="nsMessage"></param>
        internal void SetMessageProperties(IMessage message, Message nsMessage)
        {
            nsMessage.Properties = new Properties
            {
                ContentType = ContentType,
                ContentEncoding = ContentEncoding,
                CreationTime = DateTime.UtcNow
            };
            
            SetPropsFromMessageAttributes(message, nsMessage);
        }

        // INamespaceSender internal interface implementation:
        
        // The AMQP link used to send messages to a namespace item.
        public ISenderLink SenderLink { get; set; }

        // Creates the AMQP message and allows the derived class to set its
        // message specific properties.
        public Message CreateMessage(IMessage message, object body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            
            var nsMessage = new Message(body);
            
            SetMessageProperties(message, nsMessage);
            return nsMessage;
        }

        // Called to determine if the messaging being published meets the
        // criteria of the defined namespace item.
        public virtual bool MessageApplies(IMessage message)
        {
            return true;
        }

        // A message in NetFusion has a dynamic set of properties.  The following tests for a set of known
        // named values scoped to the AzurePropContext class.  If found, the value is set on the AMQP message.
        private static void SetPropsFromMessageAttributes(IMessage message, Message nsMessage)
        {
            nsMessage.Properties.CorrelationId = message.GetCorrelationId();
            nsMessage.Properties.MessageId = message.Attributes.GetValueOrDefault<string>("MessageId", null, typeof(AzurePropContext));
            nsMessage.Properties.Subject = message.Attributes.GetValueOrDefault<string>("Subject", null, typeof(AzurePropContext));
            nsMessage.Properties.To = message.Attributes.GetValueOrDefault<string>("To", null, typeof(AzurePropContext));
            nsMessage.Properties.ReplyTo = message.Attributes.GetValueOrDefault<string>("ReplyTo", null, typeof(AzurePropContext));
            
            var userId = message.Attributes.GetValueOrDefault<string>("MessageId", null, typeof(AzurePropContext));
            if (userId != null)
            {
                nsMessage.Properties.UserId =  Encoding.UTF8.GetBytes(userId);
            }
            
            var absExpiryTime = message.Attributes.GetValueOrDefault<DateTime?>("AbsoluteExpiryTime", null, typeof(AzurePropContext));
            if (absExpiryTime != null)
            {
                nsMessage.Properties.AbsoluteExpiryTime = absExpiryTime.Value;
            }
            
            var ttl = message.Attributes.GetValueOrDefault<uint?>("Ttl", null, typeof(AzurePropContext));
            if (ttl != null)
            {
                nsMessage.Header.Ttl = ttl.Value;
            }
        }
    }
}