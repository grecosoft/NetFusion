using System;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Azure.ServiceBus.Plugin
{
    /// <summary>
    /// Module service containing responsibilities for publishing of messages.
    /// </summary>
    public interface IPublisherModule : IPluginModuleService
    {
        /// <summary>
        /// Determines if there is an entity associated with a specified message
        /// type used to publish the message to Azure Service Bus.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="entity">Reference to the namespace entity.</param>
        /// <returns>Returns true if the message has an associated namespace entity.
        /// Otherwise, false.
        /// </returns>
        bool TryGetMessageEntity(Type messageType, out NamespaceEntity entity);
    }
}