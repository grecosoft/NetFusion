using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Logging;
using NetFusion.Messaging.Types.Contracts;
using NetFusion.Redis.Internal;
using NetFusion.Redis.Plugin;

namespace NetFusion.Redis.Subscriber.Internal
{
    /// <summary>
    /// Service that allows subscribing a function delegate to a channel that will be
    /// invoked when a message is published.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILogger _logger;
        private readonly IConnectionModule _connModule;
        private readonly ISerializationManager _serializationMgr;
        
        public SubscriptionService(
            ILogger<SubscriptionService> logger,
            IConnectionModule connModule,
            ISerializationManager serializationMgr)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connModule = connModule ?? throw new ArgumentNullException(nameof(connModule));
            _serializationMgr = serializationMgr ?? throw new ArgumentNullException(nameof(serializationMgr));
        }

        public void Subscribe<TDomainEvent>(string database, string channel,
            Action<TDomainEvent> handler)
            where TDomainEvent : IDomainEvent
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel not specified.", nameof(channel));

            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var subscriber = _connModule.GetSubscriber(database);
            
            subscriber.Subscribe(channel, (onChannel, message) =>
                {
                    var (contentType, messageData) = ChannelMessageEncoder.UnPack(message);

                    TDomainEvent domainEvent =_serializationMgr.Deserialize<TDomainEvent>(
                        contentType, 
                        messageData);
                    
                    LogReceivedDomainEvent(database, channel, domainEvent);
                    handler(domainEvent);
                });
        }

        public async Task SubscribeAsync<TDomainEvent>(string database, string channel,
            Action<TDomainEvent> handler) where TDomainEvent : IDomainEvent
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel not specified.", nameof(channel));

            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var subscriber = _connModule.GetSubscriber(database);
            
            await subscriber.SubscribeAsync(channel, (onChannel, message) =>
            {
                var (contentType, messageData) = ChannelMessageEncoder.UnPack(message);

                TDomainEvent domainEvent = _serializationMgr.Deserialize<TDomainEvent>(
                    contentType,
                    messageData);

                LogReceivedDomainEvent(database, channel, domainEvent);
                handler(domainEvent);
            }).ConfigureAwait(false);
        }

        private void LogReceivedDomainEvent(string database, string channel, IDomainEvent domainEvent)
        {
            _logger.LogTraceDetails(RedisLogEvents.SubscriberEvent, 
                "Subscription delegate being called.", new
                {
                    Database = database,
                    Channel = channel,
                    DomainEvent = domainEvent
                });
        }

        public void UnSubscribe(string database, string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel not specified.", nameof(channel));

            _logger.LogTrace(RedisLogEvents.SubscriberEvent, 
                "Unsubscribe channel named {channel} from database {database}", channel, database);
            
            var subscriber = _connModule.GetSubscriber(database);
            subscriber.Unsubscribe(channel);
        }

        public async Task UnSubscribeAsync(string database, string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel not specified.", nameof(channel));
                
            _logger.LogTrace(RedisLogEvents.SubscriberEvent, 
                "Unsubscribe channel named {channel} from database {database}", channel, database);
            
            var subscriber = _connModule.GetSubscriber(database);
            await subscriber.UnsubscribeAsync(channel).ConfigureAwait(false);
        }
    }
}