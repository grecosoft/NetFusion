using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;

namespace NetFusion.Azure.ServiceBus.Publisher.Internal
{
    /// <summary>
    /// Required interface implemented by all Service-Bus entity strategies.
    /// </summary>
    public interface IEntityStrategy
    {
        /// <summary>
        /// Called when service is bootstrapped to create the Service-Bus entity within
        /// a Service Bus namespace.
        /// </summary>
        /// <param name="connection">Reference to the namespace client in which
        /// the entity should be created.</param>
        /// <returns>Future Task Result.</returns>
        Task CreateEntityAsync(NamespaceConnection connection);
    }
}