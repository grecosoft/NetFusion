using System;
using EasyNetQ;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Describes a queue to be created and bound to an exchange.
    /// An instance is passed to the IQueueFactory responsible for
    /// creating and binding a queue for a specific exchange type. 
    /// </summary>
    public class QueueContext
    {
        /// <summary>
        /// Reference to the message bus on which the queue is to be created.
        /// </summary>
        public IBus Bus { get; set; }

        /// <summary>
        /// The exchange to which the created queue should be bound.
        /// </summary>
        public IExchange Exchange { get; set; }

        /// <summary>
        /// Describes the queue to be created and bound.
        /// </summary>
        public QueueDefinition Definition { get; set; }

        /// <summary>
        /// The unique value describing the host application bound to the queue.
        /// This value is appended to the base name of the queue. Applies to round
        /// robin type queues where multiple hosts (with the same HostId) will be
        /// processing messages delivered to a queue.  The HostId is the PluginId
        /// of the host application.  
        /// </summary>
        public string HostId { get; set; }

        /// <summary>
        /// Returns a queue name scoped to an unique identifier.  
        /// </summary>
        /// <returns>Queue name appended with the HostId or a globally unique value.</returns>
        public string ScopedQueueName
        {
            get
            {
                if (Definition.AppendHostId)
                {
                    return $"{Definition.QueueName}->({HostId})";
                }

                if (Definition.AppendUniqueId)
                {
                    return $"{Definition.QueueName}<-({Guid.NewGuid()})";
                }

                return Definition.QueueName;
            }
        }

        public IQueue CreateQueue()
        {
            return Bus.Advanced.QueueDeclare(
                ScopedQueueName, 
                durable: Definition.IsDurable,
                autoDelete: Definition.IsAutoDelete,
                exclusive: Definition.IsExclusive,
                passive: Definition.IsPassive,
                maxPriority: Definition.MaxPriority,
                deadLetterExchange: Definition.DeadLetterExchange,
                deadLetterRoutingKey: Definition.DeadLetterRoutingKey,
                perQueueMessageTtl: Definition.PerQueueMessageTtl);
        }
    }
}