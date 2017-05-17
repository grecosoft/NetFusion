using NetFusion.Common;
using NetFusion.Common.Extensions.Collection;
using System.Linq;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Attribute used to specify that a new queue should be created for the consumer.
    /// The method decorated with the attribute will be invoked when a new message
    /// is received by the queue. If a queue name is not specified, a generated name
    /// will be assigned.
    /// </summary>
    public class AddQueueAttribute : QueueConsumerAttribute
    {
        /// <summary>
        /// Creates new consumer queue on an exchange.   If there are externally defined
        /// queue settings associated with the queue, the QueueName property needs to be
        /// specified to the name used in the configuration.  This can be used to store
        /// the associated route keys externally.
        /// </summary>
        /// <param name="queueName">The name to assign to the queue.</param>
        /// <param name="exchangeName">The exchange on which the queue should be created.</param>
        public AddQueueAttribute(string queueName, string exchangeName) :
            base(queueName, exchangeName, QueueBindingTypes.Create)
        {
            Check.NotNullOrWhiteSpace(queueName, nameof(queueName));
            Check.NotNullOrWhiteSpace(exchangeName, nameof(exchangeName));
        }

        /// <summary>
        /// Creates a dynamically named queue on an exchange. 
        /// </summary>
        /// <param name="exchangeName">The exchange on which the queue should be created.</param>
        public AddQueueAttribute(string exchangeName) :
            base(null, exchangeName, QueueBindingTypes.Create)
        {
            Check.NotNullOrWhiteSpace(exchangeName, nameof(exchangeName));
            this.QueueSettings.IsExclusive = true;
        }

        /// <summary>
        /// A value used to determine if a message, published to the
        /// exchange, should be delivered to the queue.
        /// </summary>
        public string RouteKey
        {
            get { return this.RouteKeys.SingleOrDefault(); }
            set { this.RouteKeys = new[] { value }; }
        }

        /// <summary>
        /// Indicates that the queue should survive a server restart.
        /// </summary>
        public bool IsDurable
        {
            get { return this.QueueSettings.IsDurable; }
            set { this.QueueSettings.IsDurable = value; }
        }

        /// <summary>
        /// True if the server should delete the queue when it is no
        /// longer in use.
        /// </summary>
        public bool IsAutoDelete
        {
            get { return this.QueueSettings.IsAutoDelete; }
            set { this.QueueSettings.IsAutoDelete = value; }
        }

        /// <summary>
        /// Exclusive queues may only be accessed by the current connection,
        /// and are deleted when that connection closes.
        /// </summary>
        public bool IsExclusive
        {
            get { return this.QueueSettings.IsExclusive; }
            set { this.QueueSettings.IsExclusive = value; }
        }

        /// <summary>
        /// Indicates if the message will be automatically acknowledged
        /// when received by the consumer.  If false, the consumer must
        /// acknowledge the message.
        /// </summary>
        public bool IsNoAck
        {
            get { return this.QueueSettings.IsNoAck; }
            set { this.QueueSettings.IsNoAck = value; }
        }

        /// <summary>
        /// The max size of messages the consumer should pre-fetch.
        /// </summary>
        public uint? PrefetchSize
        {
            get { return this.QueueSettings.PrefetchSize; }
            set { this.QueueSettings.PrefetchSize = value; }
        }

        /// <summary>
        /// Number  of messages the consumer should pre-fetch.
        /// </summary>
        public ushort? PrefetchCount
        {
            get { return this.QueueSettings.PrefetchCount; }
            set { this.QueueSettings.PrefetchCount = value; }
        }
    }
}
