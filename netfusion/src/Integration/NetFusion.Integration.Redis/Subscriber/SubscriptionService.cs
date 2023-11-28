using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Base.Serialization;
using NetFusion.Integration.Redis.Internal;
using NetFusion.Integration.Redis.Plugin;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.Redis.Subscriber;

public class SubscriptionService : ISubscriptionService
{
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IConnectionModule _connModule;
    private readonly ISerializationManager _serialization;

    public SubscriptionService(
        ILogger<SubscriptionService> logger,
        IConnectionModule connModule,
        ISerializationManager serialization)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connModule = connModule ?? throw new ArgumentNullException(nameof(connModule));
        _serialization = serialization ?? throw new ArgumentNullException(nameof(serialization));
    }
    
    public void Subscribe<TDomainEvent>(string database, string channelName, Action<TDomainEvent> handler) 
        where TDomainEvent : IDomainEvent
    {
        if (string.IsNullOrWhiteSpace(database))
            throw new ArgumentException("Database name not specified.", nameof(database));
            
        if (string.IsNullOrWhiteSpace(channelName))
            throw new ArgumentException("Channel not specified.", nameof(channelName));

        if (handler == null) throw new ArgumentNullException(nameof(handler));
            
        // Obtain the corresponding database subscriber and subscribe to the channel.
        var subscriber = _connModule.GetSubscriber(database);
            
        subscriber.Subscribe(channelName, (_, value) =>
        {
            byte[]? valueData = value;
            if (valueData is null or { Length: 0})
            {
                _logger.LogError("Message received on channel: {ChannelName} was null or had zero data.", channelName);
                return;
            }
            
            var (contentType, messageData) = ChannelMessageEncoder.UnPack(valueData);

            TDomainEvent? domainEvent = _serialization.Deserialize<TDomainEvent>(
                contentType, 
                messageData);

            if (domainEvent == null)
            {
                throw new InvalidOperationException(
                    $"Message received on channel {channelName} serialized as {contentType} could not be deserialized.");
            }
                    
            LogReceivedDomainEvent(database, channelName, domainEvent);
            handler(domainEvent);
        });
    }

    public async Task SubscribeAsync<TDomainEvent>(string database, string channelName, Func<TDomainEvent, Task> handler) 
        where TDomainEvent : IDomainEvent
    {
        if (string.IsNullOrWhiteSpace(database))
            throw new ArgumentException("Database name not specified.", nameof(database));
            
        if (string.IsNullOrWhiteSpace(channelName))
            throw new ArgumentException("Channel not specified.", nameof(channelName));

        if (handler == null) throw new ArgumentNullException(nameof(handler));

        // Obtain the corresponding database subscriber and subscribe to the channel.
        var subscriber = _connModule.GetSubscriber(database);
            
        await subscriber.SubscribeAsync(channelName, (_, value) =>
        {
            byte[]? valueData = value;
            if (valueData is null or { Length: 0})
            {
                _logger.LogError("Message received on channel: {ChannelName} was null or had zero data.", channelName);
                return;
            }
            
            var (contentType, messageData) = ChannelMessageEncoder.UnPack(valueData);

            TDomainEvent? domainEvent = _serialization.Deserialize<TDomainEvent>(
                contentType,
                messageData);
                
            if (domainEvent == null)
            {
                throw new InvalidOperationException(
                    $"Message received on channel {channelName} serialized as {contentType} could not be deserialized.");
            }

            LogReceivedDomainEvent(database, channelName, domainEvent);
            handler(domainEvent).GetAwaiter().GetResult();
        }).ConfigureAwait(false);
    }

    public void UnSubscribe(string database, string channelName)
    {
        if (string.IsNullOrWhiteSpace(channelName))
            throw new ArgumentException("Channel not specified.", nameof(channelName));

        _logger.LogDebug("Unsubscribe channel named {channel} from database {database}", channelName, database);
            
        var subscriber = _connModule.GetSubscriber(database);
        subscriber.Unsubscribe(channelName);
    }

    public Task UnSubscribeAsync(string database, string channelName)
    {
        if (string.IsNullOrWhiteSpace(channelName))
            throw new ArgumentException("Channel not specified.", nameof(channelName));
                
        _logger.LogDebug("Unsubscribe channel named {channel} from database {database}", channelName, database);
            
        var subscriber = _connModule.GetSubscriber(database);
        return subscriber.UnsubscribeAsync(channelName);
    }
    
    private void LogReceivedDomainEvent(string database, string channelName, IDomainEvent domainEvent)
    {
        var channelInfo = new
        {
            Database = database,
            Channel = channelName
        };

        var log = LogMessage.For(LogLevel.Debug, "Subscription delegate being called.")
            .WithProperties(
                LogProperty.ForName("ChannelInfo", channelInfo),
                LogProperty.ForName("DomainEvent", domainEvent)
            );

        _logger.Log(log);
    }
}