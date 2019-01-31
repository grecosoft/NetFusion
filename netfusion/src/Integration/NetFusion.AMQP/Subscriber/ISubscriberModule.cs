using System.Threading.Tasks;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.AMQP.Subscriber
{
    /// <summary>
    /// Interface implemented by a class responsible for binding
    /// method handlers to host items such as queues and topics.
    /// </summary>
    public interface ISubscriberModule : IPluginModuleService
    {
        /// <summary>
        /// Uses the attribute metadata specified on handler methods
        /// and binds them to their corresponding host item and
        /// invoked when a message arrives.
        /// </summary>
        /// <param name="subscriptionSettings">The configured subscription settings
        /// provided by host application.</param>
        /// <returns>Task.</returns>
        Task LinkHandlersToHostItems(ISubscriptionSettings subscriptionSettings);
    }
}