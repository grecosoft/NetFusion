using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;

namespace NetFusion.Azure.ServiceBus.Publisher.Internal
{
    /// <summary>
    /// Optional behavior supported by an entity-strategy used to implement
    /// any associated custom Service-Bus cleanup logic.
    /// </summary>
    public interface ICleanupStrategy
    {
        /// <summary>
        /// Logic to be executed against the Service-Bus entity during service shutdown.
        /// </summary>
        /// <param name="connection">Reference to the namespace connection in which the
        /// entity should be deleted.</param>
        /// <returns>Future Task Result</returns>
        Task CleanupEntityAsync(NamespaceConnection connection);
    }
}