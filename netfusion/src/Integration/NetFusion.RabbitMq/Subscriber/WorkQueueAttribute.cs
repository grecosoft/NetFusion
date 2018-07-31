using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a command message handler should be
    /// bound to a workqueue on the default exchange.
    /// When to use:  TODO: Doc URL
    /// </summary>
    public class WorkQueueAttribute : SubscriberQueueAttribute
    {
        public WorkQueueAttribute(string queueName) : base(queueName)
        {
            QueueDefinition.SetFactory(new WorkQueueFactory());
        }
    }
}