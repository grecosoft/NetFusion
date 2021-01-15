using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Publisher.Internal;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Strategy for creating topic from configured metadata.
    /// </summary>
    public class TopicStrategy : IEntityStrategy
    {
        private readonly TopicMeta _topicMeta;

        public TopicStrategy(TopicMeta topicMeta)
        {
            _topicMeta = topicMeta;
        }

        public Task CreateEntityAsync(NamespaceConnection connection)
        {
            return connection.CreateOrUpdateTopic(_topicMeta);
        }
    }
}