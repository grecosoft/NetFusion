using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using System.Threading.Tasks;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Strategy for creating queues from configured metadata.
    /// </summary>
    public class QueueEntityStrategy : IEntityStrategy
    {
        private readonly QueueMeta _queueMeta;

        public QueueEntityStrategy(QueueMeta queueMeta)
        {
            _queueMeta = queueMeta;
        }

        public Task CreateEntityAsync(NamespaceConnection connection)
        {
            return connection.CreateOrUpdateQueue(_queueMeta);
        }
    }
}