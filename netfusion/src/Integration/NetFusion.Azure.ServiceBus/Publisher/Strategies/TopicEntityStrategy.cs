using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using System.Threading.Tasks;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Strategy for creating topic from configured metadata.
    /// </summary>
    public class TopicEntityStrategy : IEntityStrategy
    {
        private readonly TopicMeta _topicMeta;

        public TopicEntityStrategy(TopicMeta topicMeta)
        {
            _topicMeta = topicMeta;
        }

        public Task CreateEntityAsync(NamespaceConnection connection)
        {
            return connection.CreateOrUpdateTopic(_topicMeta);
        }
    }
}