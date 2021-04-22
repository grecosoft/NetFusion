using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Types.Contracts;
using NetFusion.Redis.Internal;
using NetFusion.Redis.Plugin;
using StackExchange.Redis;

namespace NetFusion.Redis.Publisher
{
    /// <summary>
    /// Extends to NetFusion base publishing pipeline to allow publishing
    /// domain-events to Redis channels.
    /// </summary>
    public class RedisPublisher : IMessagePublisher
    {
        /// <summary>
        /// Dependent Modules:
        /// </summary>
        private readonly IConnectionModule _connModule;
        private readonly IPublisherModule _pubModule;

        private readonly ILogger<RedisPublisher> _logger;
        private readonly ISerializationManager _serialization;
        private readonly IMessageLogger _messageLogger;
        
        /// <summary>
        /// Indicates that the publisher delivers messages out-of-process.
        /// </summary>
        public IntegrationTypes IntegrationType => IntegrationTypes.External;
        
        public RedisPublisher(IConnectionModule connModule, IPublisherModule pubModule,
            ILogger<RedisPublisher> logger,
            ISerializationManager serialization,
            IMessageLogger messageLogger)
        {
            _connModule = connModule ?? throw new ArgumentNullException(nameof(connModule));
            _pubModule = pubModule ?? throw new ArgumentNullException(nameof(pubModule));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
            _messageLogger = messageLogger ?? throw new ArgumentNullException(nameof(messageLogger));
        }
        
        public async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Only domain events with registered channel are published.
            if (!(message is IDomainEvent domainEvent) || !_pubModule.HasChannel(message.GetType()))
            {
                return;
            }
           
            ChannelMeta channel = _pubModule.GetChannel(domainEvent.GetType());
            
            // Only publish the domain-event if it passes the predicate 
            // associated with the channel:
            if (!channel.Applies(domainEvent))
            {
                return;
            }

            await SendEventToChannel(domainEvent, channel);
        }

        private async Task SendEventToChannel(IDomainEvent domainEvent, ChannelMeta channel)
        {
            string channelName = BuildChannelName(domainEvent, channel);

            var msgLog = new MessageLog(domainEvent, LogContextType.PublishedMessage);

            LogChannelPublish(domainEvent, channel.DatabaseName, channelName);
            AddMessageDetails(msgLog, channel.DatabaseName, channelName);

            byte[] messageValue = _serialization.Serialize(domainEvent, channel.ContentType);
            byte[] messageData = ChannelMessageEncoder.Pack(channel.ContentType, messageValue);

            try
            {
                IDatabase database = _connModule.GetDatabase(channel.DatabaseName);
                await database.PublishAsync(channelName, messageData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                msgLog.AddLogError("Redis Publisher", ex);
                throw;
            }
            finally
            {
                await _messageLogger.LogAsync(msgLog);
            }
        }

        // Build the name of the channel to publish to by combining the static channel
        // name with the optional event state data.
        private static string BuildChannelName(IDomainEvent domainEvent, ChannelMeta channel)
        {
            string eventStateData = channel.GetEventStateData(domainEvent);
            
            return string.IsNullOrWhiteSpace(eventStateData) ? channel.ChannelName 
                : $"{channel.ChannelName}.{eventStateData}";
        }
        
        private void LogChannelPublish(IDomainEvent domainEvent, string databaseName, string channelName)
        {
            _logger.LogInformation(
                "Domain event {EventName} Published to Redis Channel {ChannelName} on Database {DatabaseName}",
                domainEvent.GetType(),
                channelName, 
                databaseName);
        }

        private static void AddMessageDetails(MessageLog msgLog, string databaseName, string channelName)
        {
            msgLog.SentHint("publish-redis");
            msgLog.AddLogDetail("Database Name", databaseName);
            msgLog.AddLogDetail("Channel Name", channelName);
        }
    }
}