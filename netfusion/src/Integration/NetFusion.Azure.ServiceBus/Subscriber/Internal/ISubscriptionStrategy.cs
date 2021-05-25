using System.Threading.Tasks;
using NetFusion.Azure.ServiceBus.Namespaces;

namespace NetFusion.Azure.ServiceBus.Subscriber.Internal
{
    /// <summary>
    /// Strategy containing the logic for subscribing to a specific
    /// type of Azure Service Bus namespace entity.
    /// </summary>
    public interface ISubscriptionStrategy
    {
        /// <summary>
        /// Called when a subscription should be created on a namespace entity.
        /// </summary>
        /// <param name="connection">Connection to subscription's associated namespace.</param>
        /// <returns>Future Task Result.</returns>
        Task Subscribe(NamespaceConnection connection);
    }
}