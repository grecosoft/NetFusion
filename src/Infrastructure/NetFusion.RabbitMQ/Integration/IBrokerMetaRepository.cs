using NetFusion.RabbitMQ.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Saves the broker exchange metadata that will be used to recreate the 
    /// exchanges and queues.  The reconnection what might be a different broker
    /// behind load-balancer and therefore all exchanges and queues must be restored
    /// that are defined by both the consumer and the publisher when the reconnect.
    /// </summary>
    public interface IBrokerMetaRepository
    {
        /// <summary>
        /// Saves the broker metadata to shared location.
        /// </summary>
        /// <param name="brokers">Information needed to recreate the exchanges and queues.</param>
        /// <returns>Future result.</returns>
        Task SaveAsync(IEnumerable<BrokerMeta> brokers);

        /// <summary>
        /// Loads the broker metadata used to recreate exchanges and queues.
        /// </summary>
        /// <param name="brokerName">The name of the broker to load metadata.</param>
        /// <returns>Future result containing the broker metadata.</returns>
        Task<IEnumerable<BrokerMeta>> LoadAsync(string brokerName);
    }
}
