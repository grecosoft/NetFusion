using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Publisher.Internal;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Strategy for creating queues from configured metadata.
    /// </summary>
    public class QueueStrategy : IEntityStrategy
    {
        private readonly QueueMeta _queueMeta;

        public QueueStrategy(QueueMeta queueMeta)
        {
            _queueMeta = queueMeta;
        }

        public Task CreateEntityAsync(NamespaceConnection connection)
        {
            return connection.CreateOrUpdateQueue(_queueMeta);
        }
    }
}