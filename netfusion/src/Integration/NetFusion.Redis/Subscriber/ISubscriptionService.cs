using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Redis.Subscriber
{
    /// <summary>
    /// Service that can be injected into a singleton registered service
    /// used to dynamically subscribe and unsubscribe from Redis channels.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Subscribes and event handler to a channel.
        /// </summary>
        /// <param name="database">The name of the configured database.</param>
        /// <param name="channel">The channel to subscribe.</param>
        /// <param name="handler">The handler to invoke when a domain-event
        /// is received on the channel.</param>
        /// <typeparam name="TDomainEvent">The type of domain event to
        /// deserialized received data into.</typeparam>
        void Subscribe<TDomainEvent>(string database, string channel,
            Action<TDomainEvent> handler)        
            where TDomainEvent : IDomainEvent;

        /// <summary>
        /// Subscribes and event handler to a channel.
        /// </summary>
        /// <param name="database">The name of the configured database.</param>
        /// <param name="channel">The channel to subscribe.</param>
        /// <param name="handler">The handler to invoke when a domain-event
        ///     is received on the channel.</param>
        /// <typeparam name="TDomainEvent">The type of domain event to
        /// deserialized received data into.</typeparam>
        Task SubscribeAsync<TDomainEvent>(string database, string channel,
            Action<TDomainEvent> handler)        
            where TDomainEvent : IDomainEvent;

        /// <summary>
        /// Unsubscribe all handlers from the specified channel.
        /// </summary>
        /// <param name="database">The name of the configured database.</param>
        /// <param name="channel">The channel to unsubscribe.</param>
        void UnSubscribe(string database, string channel);

        /// <summary>
        /// Unsubscribe all handlers from the specified channel.
        /// </summary>
        /// <param name="database">The name of the configured database.</param>
        /// <param name="channel">The channel to unsubscribe.</param>
        Task UnSubscribeAsync(string database, string channel);
    }
}