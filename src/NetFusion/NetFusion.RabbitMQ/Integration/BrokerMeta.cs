using System.Collections.Generic;

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
    public class BrokerMeta
    {
        public string BrockerConfigId { get; set; }
        public string ApplicationId { get; set; }
        public string BrokerName { get; set; }
        public ICollection<ExchangeMeta> ExchangeMeta { get; set; }
    }
}
