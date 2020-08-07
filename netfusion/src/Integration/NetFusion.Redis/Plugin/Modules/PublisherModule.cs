using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Redis.Publisher;

namespace NetFusion.Redis.Plugin.Modules
{
    /// <summary>
    /// Plugin module that scans for all IChannelRegistry instances and builds a list of 
    /// metadata used to determine the channels to which a domain-event should be published.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {
        private IEnumerable<IChannelRegistry> Registries { get; set; }
        
        // Maps a domain-event type to a specific channel instance.
        private Dictionary<Type, ChannelMeta> _channels;

        public override void Initialize()
        {
            ChannelMeta[] eventPubChannels = Registries
                .SelectMany(r => r.GetChannels())
                .ToArray();
            
            AssertChannels(eventPubChannels);

            _channels = eventPubChannels.ToDictionary(c => c.DomainEventType);
        }

        private static void AssertChannels(ChannelMeta[] channels)
        {
            var duplicateChannels = channels.WhereDuplicated(m => m.ChannelName);

            if (duplicateChannels.Any())
            {
                throw new ContainerException(
                    "Channel names must be unique.", "duplicate-channels", duplicateChannels);
            }

            var duplicateEventTypes = channels.WhereDuplicated(m => m.DomainEventType)
                .Select(t => t.FullName);

            if (duplicateEventTypes.Any())
            {
                throw new ContainerException(
                    "Domain event types can only be associated with one channel.", 
                    "duplicate-events", duplicateEventTypes);
            }
        }

        public bool HasChannel(Type domainEventType)
        {
            if (domainEventType == null) throw new ArgumentNullException(nameof(domainEventType));
            return _channels.ContainsKey(domainEventType);
        }

        public ChannelMeta GetChannel(Type domainEventType)
        {
            if (domainEventType == null) throw new ArgumentNullException(nameof(domainEventType));
            
            if (! HasChannel(domainEventType))
            {
                throw new InvalidOperationException(
                    $"A channel is not configured for domain event type: {domainEventType}.");
            }

            return _channels[domainEventType];
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["EventChannels"] = _channels.Values
                .Select(c => new
                {
                    EventType = c.DomainEventType.FullName,
                    Channel = c.ChannelName
                }).ToArray();
        }
    }
}
