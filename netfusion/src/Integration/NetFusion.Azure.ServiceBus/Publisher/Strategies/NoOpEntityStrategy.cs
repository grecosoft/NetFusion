using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using System.Threading.Tasks;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Default strategy implementation.
    /// </summary>
    public class NoOpEntityStrategy : IEntityStrategy
    {
        public Task CreateEntityAsync(NamespaceConnection connection) => Task.CompletedTask;
    }
}