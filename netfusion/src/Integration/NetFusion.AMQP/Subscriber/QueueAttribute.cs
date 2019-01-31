using System;
using NetFusion.AMQP.Subscriber.Internal;

namespace NetFusion.AMQP.Subscriber
{
    /// <summary>
    /// Attribute used to specify a queue defined on a namespace that
    /// should be invoked when a message arrives.
    /// </summary>
    public class QueueAttribute : HostItemAttribute
    {
        /// <summary>
        /// The name of the defined queue.
        /// </summary>
        public string QueueName { get; }
        
        public QueueAttribute(string queue)
            : base (new QueueSubscriberLinker())
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name not specified.", nameof(queue));
            
            QueueName = queue;
        }
    }
}