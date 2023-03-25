using Microsoft.Extensions.Logging;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.Redis.Internal;
using NetFusion.Integration.Redis.Publisher.Metadata;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;
using StackExchange.Redis;

namespace NetFusion.Integration.Redis.Publisher.Strategies;

public class ChannelPublishStrategy : BusEntityStrategyBase<ChannelEntityContext>,
    IBusEntityPublishStrategy
{
    private readonly ChannelPublisherEntity _entity;

    public ChannelPublishStrategy(ChannelPublisherEntity entity) : base(entity)
    {
        _entity = entity;
    }

    public bool CanPublishMessageType(Type messageType) => messageType == _entity.DomainEventType;
    
    private ILogger<ChannelPublishStrategy> Logger =>
        Context.LoggerFactory.CreateLogger<ChannelPublishStrategy>();

    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        string channelName = BuildChannelName(message, _entity.PublishMeta);
        string contentType = message.GetContentType() ?? _entity.PublishMeta.ContentType;
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        LogChannelPublish(message, channelName);
        AddMessageDetails(msgLog, channelName);
        
        byte[] messageValue = Context.Serialization.Serialize(message, contentType);
        byte[] messageData = ChannelMessageEncoder.Pack(contentType, messageValue);

        try
        {
            IDatabase database = Context.ConnectionModule.GetDatabase(_entity.BusName);
            await database.PublishAsync(channelName, messageData).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError("Redis Publisher", ex);
            throw;
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    // Build the name of the channel to publish to by combining the static channel
    // name with the optional event state data.
    private static string BuildChannelName(IMessage domainEvent, PublishMeta publishMeta)
    {
        string eventStateData = publishMeta.GetEventStateData(domainEvent);
        
        return string.IsNullOrWhiteSpace(eventStateData) ? publishMeta.ChannelName! 
            : $"{publishMeta.ChannelName}.{eventStateData}";
    }
    
    private void LogChannelPublish(IMessage domainEvent, string channelName)
    {
        Logger.LogDebug(
            "Domain event {EventName} Published to Redis Channel {ChannelName} on Database {DatabaseName}",
            _entity.DomainEventType.Name,
            channelName, 
            _entity.BusName);
    }

    private void AddMessageDetails(MessageLog msgLog, string channelName)
    {
        msgLog.SentHint("publish-redis");
        msgLog.AddLogDetail("Database Name", _entity.BusName);
        msgLog.AddLogDetail("Channel Name", channelName);
    }
}