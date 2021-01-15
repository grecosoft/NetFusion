using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Publisher.Internal;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Default strategy implementation used for entities with no associated
    /// Service-Bus entities.
    /// </summary>
    public class NoOpStrategy : IEntityStrategy
    {
        public Task CreateEntityAsync(NamespaceConnection connection) => Task.CompletedTask;
    }
}