using System;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Settings to be used when declaring the queue on an exchange.
    /// </summary>
    public class QueueSettings 
    {
        public QueueSettings()
        {
            this.Arguments = new Dictionary<string, object>();
        }

        /// <summary>
        /// Indicates that the queue should survive a server restart.
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// The queue is automatically deleted when the last consumer un-subscribes.
        /// If you need a temporary queue used only by one consumer, combine auto-delete
        /// with exclusive.  When the consumer disconnects, the queue will be removed.
        /// </summary>
        public bool IsAutoDelete { get; set; }

        /// <summary>
        /// When set to true, your queue becomes private and can only be consumed by your
        /// application.  This is useful when you need to limit a queue to only one consumer.
        /// </summary>
        public bool IsExclusive { get; set; }

        /// <summary>
        /// The broker will generate a unique name for the queue.
        /// </summary>
        public bool IsBrokerAssignedName { get; set; }

        /// <summary>
        /// When auto_ack is specified, RabbitMQ will automatically consider the message
        /// acknowledged by the consumer as soon as the consumer has received it.
        /// </summary>
        public bool IsNoAck { get; set; }

        public uint? PrefetchSize { get; set; }

        public ushort? PrefetchCount { get; set; }

        /// <summary>
        /// Other properties (construction arguments) for the queue.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Clones the default queue settings so they can be modified
        /// for each created queue.
        /// </summary>
        /// <returns>Copy of the settings.</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
