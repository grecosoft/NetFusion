using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// Meta-data for a saved exchange's queue.
    /// </summary>
    public class QueueConfig
    {
        public string QueueName { get; set; }
        public string[] RouteKeys { get; set; }
        public QueueSettings Settings { get; set; }
    }
}
