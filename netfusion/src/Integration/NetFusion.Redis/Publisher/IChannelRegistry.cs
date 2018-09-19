using System.Collections.Generic;
using NetFusion.Base.Plugins;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Called during the bootstrap process to determine which domain-events
    /// should be delivered to a Redis channel when published.
    /// </summary>
    public interface IChannelRegistry : IKnownPluginType
    {
        /// <summary>
        /// Returns a list of domain-event to channel mappings.
        /// </summary>
        /// <returns>Application configured mappings.</returns>
        IEnumerable<ChannelMeta> GetChannels();
    }
}
