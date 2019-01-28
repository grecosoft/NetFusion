using System;
using System.Collections.Generic;
using NetFusion.RabbitMQ.Settings;
using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Metadata
{
    /// <summary>
    /// Contains metadata describing a broker queue.
    /// </summary>
    public class QueueMeta
    {
        /// <summary>
        /// The name of the queue. 
        /// </summary>
        public string QueueName { get; private set; }
        
        /// <summary>
        /// The name of the queue with an unique GUID appended or the
        /// GUID identifying the consuming host application.
        /// </summary>
        public string ScopedQueueName { get; private set; }
        
        /// <summary>
        /// The metadata of the exchange associated with the queue on which
        /// it should be created.
        /// </summary>
        public ExchangeMeta Exchange { get; private set; }
        
        /// <summary>
        /// Defines a queue on an exchange.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="exchange">Reference to the exchange on which it should be created.</param>
        /// <param name="config">Delegate used to specify additional queue metadata.</param>
        /// <returns>The queue metadata.</returns>
        public static QueueMeta Define(string queueName, ExchangeMeta exchange, Action<QueueMeta> config = null)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name not specified.", nameof(queueName));
            if (exchange == null) throw new ArgumentNullException(nameof(exchange));
            
            var queue = new QueueMeta
            {
               QueueName = queueName,
               ScopedQueueName = queueName,
               Exchange = exchange
            };
            
            config?.Invoke(queue);
            return queue;
        }

        /// <summary>
        /// Value containing the queue's base name appended with an unique identifier.
        /// </summary>
        /// <param name="scopedName">The associated scoped name.</param>
        internal void SetScopedName(string scopedName)
        {
            if (string.IsNullOrWhiteSpace(scopedName))
                throw new ArgumentException("Scoped queue name not specified.", nameof(scopedName));
            
            ScopedQueueName = scopedName;
        }
        
        /// <summary>
        /// Reference to class specified on the subscribing side used to determine how
        /// the metadata for a specific queue type and how a message received on the
        /// queue should be processed.
        /// </summary>
        internal IQueueFactory QueueFactory { get; set; }

        /// <summary>
        /// Do not create an exchange. If the named exchange doesn't exist, throw an exception.
        /// </summary>
        public bool IsPassive { get; set; }

        /// <summary>
        /// Indicates that the queue should survive a server restart.
        /// </summary>
        public bool IsDurable { get; set; } 

        /// <summary>
        /// Determines if multiple clients can monitor queue.
        /// </summary>
        public bool IsExclusive { get; set; } 

        /// <summary>
        /// The queue is automatically deleted when the last consumer un-subscribes.
        /// If you need a temporary queue used only by one consumer, combine auto-delete
        /// with exclusive.  When the consumer disconnects, the queue will be removed.
        /// </summary>
        public bool IsAutoDelete { get; set; }

        /// <summary>
        /// Indicates that the unique identity value representing the host should be
        /// appended to the queue name.  
        /// </summary>
        public bool AppendHostId { get; set; } 

        /// <summary>
        /// Indicates that an unique id should be appended to the name of the queue.
        /// </summary>
        public bool AppendUniqueId { get; set; }
        
        /// <summary>
        /// Applies queue settings specified within the application's configuration
        /// to the queue metadata.  Only values specified are set.
        /// </summary>
        /// <param name="settings">External stored exchange settings.</param>
        public void ApplyOverrides(QueueSettings settings)
        {
            IsPassive = settings.Passive ?? IsPassive;
            RouteKeys = settings.RouteKeys ?? RouteKeys;

            PrefetchCount = settings.PrefetchCount ?? PrefetchCount;
            MaxPriority = settings.MaxPriority;
            Priority = settings.Priority ?? Priority;
            
            DeadLetterExchange = settings.DeadLetterExchange ?? DeadLetterExchange;
            DeadLetterRoutingKey = settings.DeadLetterRoutingKey ?? DeadLetterRoutingKey;
            PerQueueMessageTtl = settings.PerQueueMessageTtl ?? PerQueueMessageTtl;
        }
                
        /// <summary>
        /// Determines the maximum message priority that the queue should support.
        /// </summary>
        public byte? MaxPriority { get; set; }

        /// <summary>
        /// How long in milliseconds a message should remain on the queue before it is discarded.
        /// </summary>
        public int? PerQueueMessageTtl { get; set;}

        /// <summary>
        ///  Determines an exchange name can remain unused before it is automatically deleted by the server.
        /// </summary>
        public string DeadLetterExchange { get; set;}

        /// <summary>
        /// Determines an exchange name can remain unused before it is automatically deleted by the server.
        /// </summary>
        public string DeadLetterRoutingKey { get; set;}

        /// <summary>
        /// The route keys if the specified type of exchange supports the usage of route keys.
        /// </summary>
        public string[] RouteKeys { get; set; }

        /// <summary>
        /// This is the number of messages that will be delivered by RabbitMQ before an ack is sent by EasyNetQ.
        /// Set to 0 for infinite prefetch (not recommended). Set to 1 for fair work balancing among a farm of consumers.
        /// </summary>
        public ushort PrefetchCount { get; set; } = 50;

        public int Priority { get; set; }

        /// <summary>
        /// Adds queue log information to the dictionary of values.
        /// </summary>
        /// <param name="log">Dictionary containing log values.</param>
        public void LogProperties(IDictionary<string, object> log)
        {
            log["Queue"] = GetLogDetails();
            log["Exchange"] = Exchange.IsDefaultExchange ? "Default-Exchange" : Exchange.GetLogDetails();
        }

        /// <summary>
        /// Returns an anonymous type containing the queue properties to be logged.
        /// </summary>
        /// <returns>Object with properties to be logged.</returns>
        public object GetLogDetails()
        {
            return new
            {
                QueueName,
                ScopedQueueName,
                IsAutoDelete,
                IsDurable,
                IsExclusive,
                IsPassive,
                RouteKeys,
                Priority,
                MaxPriority,
                PerQueueMessageTtl,
                DeadLetterRoutingKey,
                PrefetchCount
            };
        }
    }
}