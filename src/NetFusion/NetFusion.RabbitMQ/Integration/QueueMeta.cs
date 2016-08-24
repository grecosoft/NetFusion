using NetFusion.RabbitMQ.Core;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// The queue metadata needed to recreate the queue.
    /// </summary>
    public class QueueMeta
    {
        public string QueueName { get; set; }
        public string[] RouteKeys { get; set; }
        public QueueSettings Settings { get; set; }
    }
}
