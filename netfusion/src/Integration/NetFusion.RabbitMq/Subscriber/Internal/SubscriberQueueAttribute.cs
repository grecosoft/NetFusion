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
        public string BusName { get;  }
        public string QueueName { get;  }
        public IQueueFactory QueueFactory { get; }
        
        public string ExchangeName { get; protected set; }
        public string[] RouteKeys { get; protected set; }

        protected SubscriberQueueAttribute(string busName, string queueName, IQueueFactory factory)
        {
            BusName = busName;
            QueueName = queueName;
            QueueFactory = factory;
        }
    }
}