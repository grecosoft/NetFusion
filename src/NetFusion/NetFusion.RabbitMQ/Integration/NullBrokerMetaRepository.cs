using NetFusion.RabbitMQ.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Integration
{
    class NullBrokerMetaRepository : IBrokerMetaRepository
    {
        public Task<IEnumerable<BrokerMeta>> LoadAsync(string brokerName)
        {
            return Task.FromResult<IEnumerable<BrokerMeta>>(new BrokerMeta[] { });
        }

        public Task SaveAsync(IEnumerable<BrokerMeta> brokers)
        {
            return Task.FromResult<object>(null);
        }
    }
}
