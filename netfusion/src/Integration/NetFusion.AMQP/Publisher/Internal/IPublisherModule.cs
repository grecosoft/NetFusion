using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.AMQP.Publisher.Internal
{
    /// <summary>
    /// Contract for module responsible for determining the host items
    /// (i.e. Queues/Topics) to which messages can be sent.
    /// </summary>
    public interface IPublisherModule : IPluginModuleService
    {
        /// <summary>
        /// Determines if a message type is associated with a host
        /// define item.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>True if a host item is associated with the message type.</returns>
        bool HasHostItem(Type messageType);

        /// <summary>
        /// Return the host item associated with a specific message type.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <returns>The host item.  If not items is associated with the
        /// message type, an exception is thrown.</returns>
        IHostItem GetHostItem(Type messageType);
    }
}