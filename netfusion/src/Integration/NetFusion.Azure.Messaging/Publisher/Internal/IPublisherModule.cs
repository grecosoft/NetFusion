using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Azure.Messaging.Publisher.Internal
{
    /// <summary>
    /// Contract for module responsible for determining the Azure namespace
    /// items (i.e. Queues/Topics) to which the host will send messages.
    /// </summary>
    public interface IPublisherModule : IPluginModuleService
    {
        /// <summary>
        /// Determines if a message type is associated with a namespace
        /// define object.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>True if a namespace object is associated with the message type.</returns>
        bool HasNamespaceItem(Type messageType);

        /// <summary>
        /// Return the namespace item associated with a specific message type.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>The namespace item.  If not items is associated with the
        /// message type, an exception is thrown.</returns>
        INamespaceItem GetNamespaceItem(Type messageType);
    }
}