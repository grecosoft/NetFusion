using System.Collections.Generic;
using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// Class containing exchange meta-data for all exchanges and their
    /// corresponding queues that were defined by publishers.  In the
    /// case of a node failure, in a RabbitMQ cluster, the client that
    /// handles the exception must reestablish the exchanges and queues
    /// on the new node.  Since producers and consumers are will often
    /// not be defined within the same application, this meta-data is
    /// stored.
    /// </summary>
    public class ExchangeConfig
    {
        public string ExchangeConfigId { get; set; }
        public string BrokerName { get; set; }
        public string ExchangeName { get; set; }
        public ExchangeSettings Settings { get; set; }
        public ICollection<QueueConfig> QueueConfigs { get; set; }
    }
}
