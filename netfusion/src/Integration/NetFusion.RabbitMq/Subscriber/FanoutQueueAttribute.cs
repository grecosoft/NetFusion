using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a message handler should be bound to a queue
    /// on a fanout exchange.
    /// When to use:  TODO: Doc URL
    /// </summary>
    public class FanoutQueueAttribute : SubscriberQueueAttribute
    {
        public FanoutQueueAttribute(string queueName, string exchangeName) 
            : base(queueName, exchangeName)
        {
            QueueDefinition.SetFactory(new FanoutQueueFactory());
        }
    }
}