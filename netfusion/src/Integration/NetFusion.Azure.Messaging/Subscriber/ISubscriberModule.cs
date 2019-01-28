using System.Threading.Tasks;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Azure.Messaging.Subscriber
{
    /// <summary>
    /// Interface implemented by a class responsible for binding
    /// method handlers to namespace items such as queues and topics.
    /// </summary>
    public interface ISubscriberModule : IPluginModuleService
    {
        /// <summary>
        /// Uses the attribute metadata specified on handler methods
        /// and binds them to their corresponding namespace item and
        /// are invoked when a message arrives.
        /// </summary>
        /// <param name="subscriptionSettings">The configured subscription settings
        /// provided by host application.</param>
        /// <returns>Task.</returns>
        Task LinkHandlersToNamespaces(ISubscriptionSettings subscriptionSettings);
    }
}