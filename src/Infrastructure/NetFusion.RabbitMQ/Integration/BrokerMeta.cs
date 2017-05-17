using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// Class containing exchange meta-data for all exchanges and their
    /// corresponding queues that were defined by publishers.  In the
    /// case of a node failure, in a RabbitMQ cluster, the client that
    /// handles the exception must reestablish the exchanges and queues
    /// on the new node.  Since producers and consumers will often not 
    /// be defined within the same application, this meta-data is stored
    /// externally.
    /// </summary>
    public class BrokerMeta
    {
        /// <summary>
        /// Database generated key.
        /// </summary>
        public string BrockerConfigId { get; set; }

        /// <summary>
        /// The application defining the saved exchanges and queues.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// The name of the broker on which the exchanges and queues
        /// are created.
        /// </summary>
        public string BrokerName { get; set; }

        /// <summary>
        /// The exchange metadata needed to recreate the exchange.
        /// </summary>
        public ICollection<ExchangeMeta> ExchangeMeta { get; set; }
    }
}
