using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Service module responsible for determining which domain-events
    /// when published should be sent to a given Redis channel.
    /// </summary>
    public interface IPublisherModule : IPluginModuleService
    {
        /// <summary>
        /// Determines if the type of domain-event is associated with a channel.
        /// </summary>
        /// <param name="domainEventType">The type of domain-event.</param>
        /// <returns>True if associated with channel.  Otherwise, false.</returns>
        bool HasChannel(Type domainEventType);
        
        /// <summary>
        /// Returns metadata describing the channel associated with a given
        /// domain-event type.
        /// </summary>
        /// <param name="domainEventType">The type of domain-event.</param>
        /// <returns>The associated channel metadata.  If metadata is not
        /// found, an exception is raised.</returns>
        ChannelMeta GetChannel(Type domainEventType);
    }
}
