using System;
using NetFusion.Base;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Class containing information about a domain-event and the database channel
    /// to which it should be sent when a event of the type is published.
    /// </summary>
    public abstract class ChannelMeta
    {
        public Type DomainEventType { get; }
        public string DatabaseName { get; }
        public string ChannelName { get; }

        protected ChannelMeta(Type domainEventType, string database, string channel)
        {
            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentException("Database name for channel not specified.", nameof(database));
            
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel name not specified.", nameof(channel));

            DomainEventType = domainEventType ?? throw new ArgumentNullException(nameof(domainEventType));
            DatabaseName = database;
            ChannelName = channel;
        }
        
        /// <summary>
        /// The content type of the serialized message.
        /// </summary>
        public string ContentType { get; protected set; } = ContentTypes.Json;
        
        // Determines if the message should be sent to the channel.
        internal virtual bool Applies(IDomainEvent domainEvent) => true;

        internal abstract string GetEventStateData(IDomainEvent domainEvent);
    }
}