using System;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Base attribute used to decorate message handlers that are to be bound
    /// to a queue and invoked when a message is delivered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class SubscriberQueueAttribute : Attribute
    {
        internal QueueDefinition QueueDefinition { get; }
        internal QueueExchangeDefinition ExchangeDefinition { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="exchangeName">The name of the exchange.  If the queue
        /// is associated with the default exchange, a value need not be specified.</param>
        protected SubscriberQueueAttribute(string queueName, string exchangeName = null)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("Queue name must be specified.", nameof(queueName));
            }

            QueueDefinition = new QueueDefinition { QueueName = queueName };
            ExchangeDefinition = new QueueExchangeDefinition { ExchangeName = exchangeName };
        }

        /// <summary>
        /// The name of the bus configured within the application's settings.  If not 
        /// specified, the default queue named:  message-bus will be used.
        /// </summary>
        public string BusName
        {
            get => ExchangeDefinition.BusName;
            set => ExchangeDefinition.BusName = value; 
        }
    }
}