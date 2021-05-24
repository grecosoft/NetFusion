using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Publisher.Internal;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Default strategy implementation used to reference an entity published
    /// to but created by another Microservice.
    /// </summary>
    public class NoOpStrategy : IEntityStrategy
    {
        public Task CreateEntityAsync(NamespaceConnection connection) => Task.CompletedTask;
    }
}