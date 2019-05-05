using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Logging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using NetFusion.Redis.Internal;
using NetFusion.Redis.Plugin;
using StackExchange.Redis;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Extends to NetFusion base publishing pipeline to allow publishing
    /// domain-events to Redis channels.
    /// </summary>
    public class RedisPublisher : MessagePublisher
    {
        /// <summary>
        /// Dependent Modules:
        /// </summary>
        private readonly IConnectionModule _connModule;
        private readonly IPublisherModule _pubModule;

        private readonly ILogger _logger;
        private readonly ISerializationManager _serialization;
        
        /// <summary>
        /// Indicates that the publisher delivers messages out-of-process.
        /// </summary>
        public override IntegrationTypes IntegrationType => IntegrationTypes.External;
        
        public RedisPublisher(IConnectionModule connModule, IPublisherModule pubModule,
            ILogger<RedisPublisher> logger,
            ISerializationManager serialization)
        {
            _connModule = connModule ?? throw new ArgumentNullException(nameof(connModule));
            _pubModule = pubModule ?? throw new ArgumentNullException(nameof(pubModule));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
        }
        
        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (message is IDomainEvent domainEvent && _pubModule.HasChannel(message.GetType()))
            {
                ChannelMeta channel = _pubModule.GetChannel(domainEvent.GetType());
                IDatabase database = _connModule.GetDatabase(channel.DatabaseName);
                
                // Only publish the domain-event if it passes the predicate associated
                // with the channel:
                if (! channel.Applies(domainEvent))
                {
                    return;
                }
                          
                byte[] messageValue = _serialization.Serialize(domainEvent, channel.ContentType);
                
                // Build the name of the channel to publish to by combining the static channel
                // name with the optional event state data.
                string eventStateData = channel.GetEventStateData(domainEvent);
                string channelName = $"{channel.ChannelName}.{eventStateData}";

                LogChannelPublish(domainEvent, channel.DatabaseName, channelName);
                byte[] messageData = ChannelMessageEncoder.Pack(channel.ContentType, messageValue);

                await database.PublishAsync(channelName, messageData).ConfigureAwait(false);
            }
        }
        
        private void LogChannelPublish(IDomainEvent domainEvent, string databaseName, string channelName)
        {
            _logger.LogTraceDetails(RedisLogEvents.PublisherEvent, "Domain Event being published to Redis Channel.", 
                new
                {
                    DatabaseConfigName = databaseName,
                    ChannelName = channelName,
                    DomainEvent = domainEvent
                });
        }
    }
}