using System.Collections.Generic;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Class used by publishers to indicate the channel to which a given
    /// domain-event should be published.
    /// </summary>
    public abstract class ChannelRegistryBase : IChannelRegistry
    {
        private readonly List<ChannelMeta> _channels = new List<ChannelMeta>();

        public IEnumerable<ChannelMeta> GetChannels()
        {
            OnRegister();
            return _channels;
        }

        /// <summary>
        /// Called when the application is bootstrapped to allow application specific
        /// derived classes to register domain-events with a specific channel.
        /// </summary>
        protected abstract void OnRegister();

        protected ChannelMeta<TDomainEvent> AddChannel<TDomainEvent>(string databaseName, string channel)
            where TDomainEvent : IDomainEvent
        {
            var meta = new ChannelMeta<TDomainEvent>(databaseName, channel);
            _channels.Add(meta);
            return meta;
        }
    }
}
