using NetFusion.RabbitMQ.Exchanges;
using System;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Used to consume messages delivered to a queue on an exchange.  This attribute
    /// is applied to consumer methods to indicate that the method should be called 
    /// when the corresponding message is received on the queue.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class QueueConsumerAttribute : Attribute
    {
        public string QueueName { get; }
        public string ExchangeName { get; }
        public QueueBindingTypes BindingType { get; }

        protected QueueConsumerAttribute(string queueName, string exchangeName,
            QueueBindingTypes bindingType)
        {
            this.ExchangeName = exchangeName;
            this.QueueName = queueName;
            this.BindingType = bindingType;

            this.QueueSettings = new QueueSettings { IsBrokerAssignedName = queueName == null };
        }

        /// <summary>
        /// One or more values used to determine if a messaged, published
        /// to the exchange, should be delivered to the queue.
        /// </summary>
        public string[] RouteKeys { get; protected set; }

        internal QueueSettings QueueSettings { get; }
    }
}
