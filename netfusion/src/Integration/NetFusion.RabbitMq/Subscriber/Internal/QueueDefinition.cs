using System;
using EasyNetQ.Topology;
using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Describes a queue to be bound to an exchange and subscribed to by the consumer.
    /// </summary>
    public class QueueDefinition
    {
        /// <summary>
        /// The name of the queue. 
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        ///  Do not create an exchange. If the named exchange doesn't exist, throw an exception.
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
        /// Determines the maximum message priority that the queue should support.
        /// </summary>
        public byte? MaxPriority { get; set; }

        /// <summary>
        /// How long in milliseconds a message should remain on the queue before it is discarded.
        /// </summary>
        public int? PerQueueMessageTtl { get; set;}

        /// <summary>
        ///  Determines an exchange's name can remain unused before it is automatically deleted by the server.
        /// </summary>
        public string DeadLetterExchange { get; set;}

        /// <summary>
        /// Determines an exchange's name can remain unused before it is automatically deleted by the server.
        /// </summary>
        public string DeadLetterRoutingKey { get; set;}

        /// <summary>
        /// The route keys if the specified type of exchange supports the usage of route keys.
        /// </summary>
        public string[] RouteKeys { get; private set; }

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
        /// This is the number of messages that will be delivered by RabbitMQ before an ack is sent by EasyNetQ.
        /// Set to 0 for infinite prefetch (not recommended). Set to 1 for fair work balancing among a farm of consumers.
        /// </summary>
        public ushort PrefetchCount { get; set; }

        public int Priority { get; set; }

        /// <summary>
        /// Responsible for creating and binding a queue to a specific type of exchange. 
        /// </summary>
        public IQueueFactory QueueFactory { get; private set; }

        /// <summary>
        /// The route keys for which the queue to be bound to the exchange. 
        /// </summary>
        /// <param name="routeKeys">List of route-keys.</param>
        public QueueDefinition WithRouteKey(params string[] routeKeys)
        {
            RouteKeys = routeKeys ?? throw new ArgumentNullException(nameof(routeKeys));
            return this;
        }

        /// <summary>
        /// Sets the factory responsible for creating a specific type of queue.
        /// </summary>
        /// <param name="factory">Factory instance.</param>
        public void SetFactory(IQueueFactory factory)
        {
            QueueFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Delegates to the specified factory to create an instance of a queue
        /// based on the settings specified within a passed context.
        /// </summary>
        /// <param name="context">Contains details about the queue to be created.</param>
        /// <returns>The created queue.</returns>
        public IQueue CreateQueue(QueueContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (QueueFactory == null)
            {
                throw new InvalidOperationException("The queue factory has not been set.");
            }

            return QueueFactory.CreateQueue(context);
        }

        // Overrides the default conventions topically used for a given queue type
        // with values specified within the application's configuration.
        public void ApplyOverrides(QueueSettings configuredSettings)
        {
            IsPassive = configuredSettings.Passive;
            PerQueueMessageTtl = configuredSettings.PerQueueMessageTtl;
            MaxPriority = configuredSettings.MaxPriority;
            DeadLetterExchange = configuredSettings.DeadLetterExchange;
            DeadLetterRoutingKey = DeadLetterRoutingKey;

            RouteKeys = configuredSettings.RouteKeys;
            PrefetchCount = configuredSettings.PrefetchCount;
            Priority = configuredSettings.Priority;
        }
    }
}