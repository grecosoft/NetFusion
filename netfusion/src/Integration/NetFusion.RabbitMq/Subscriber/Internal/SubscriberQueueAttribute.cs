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
        public string BusName { get; }
        public string QueueName { get;  }
        public IQueueStrategy QueueStrategy { get; }
        
        public string ExchangeName { get; protected set; }
        public string[] RouteKeys { get; protected set; }
        public bool IsNonRoutedSaved { get; set; }
        public bool IsUnacknowledgedSaved { get; set; }

        protected SubscriberQueueAttribute(string busName, string queueName, IQueueStrategy strategy)
        {
            if (string.IsNullOrWhiteSpace(busName))
                throw new ArgumentException("Bus name not specified.", nameof(busName));

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(busName));

            BusName = busName;
            QueueName = queueName;
            QueueStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }
    }
}