using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Contains information about a queue to be created
    /// on an exchange.
    /// </summary>
    public class ExchangeQueue
    {
        /// <summary>
        /// The name of the queue.
        /// </summary>
        public string QueueName { get; internal set; }

        /// <summary>
        /// The route keys if the specified type of exchange supports
        /// the usage of route keys.
        /// </summary>
        public string[] RouteKeys { get; set; }

        /// <summary>
        /// The setting to use when declaring the queue.
        /// </summary>
        public QueueSettings Settings { get; internal set; }
    }
}  
