using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a command message handler should be
    /// bound to a workqueue on the default exchange.
    /// </summary>
    public class WorkQueueAttribute : SubscriberQueueAttribute
    {
        public WorkQueueAttribute(string busName, string queueName) 
            
            : base(busName, queueName, new WorkQueueFactory())
        {
            
        }
    }
}