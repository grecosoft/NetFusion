using NetFusion.RabbitMQ.Core;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// Meta-data for a saved exchange's queue.
    /// </summary>
    public class QueueMeta
    {
        public string QueueName { get; set; }
        public string[] RouteKeys { get; set; }
        public QueueSettings Settings { get; set; }
    }
}
