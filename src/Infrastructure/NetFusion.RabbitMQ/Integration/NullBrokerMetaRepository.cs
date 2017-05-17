using NetFusion.RabbitMQ.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// Null implementation that is registered by default if exchange
    /// and queue information is not to be saved.
    /// </summary>
    public class NullBrokerMetaRepository : IBrokerMetaRepository
    {
        public Task<IEnumerable<BrokerMeta>> LoadAsync(string brokerName)
        {
            return Task.FromResult<IEnumerable<BrokerMeta>>(new BrokerMeta[] { });
        }

        public Task SaveAsync(IEnumerable<BrokerMeta> brokers)
        {
            return Task.CompletedTask;
        }
    }
}
