using System.ComponentModel;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Logging;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.Redis.Internal;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Types.Contracts;
using StackExchange.Redis;

namespace NetFusion.Integration.Redis.Subscriber.Strategies;

public class ChannelSubscriberStrategy(ChannelSubscriberEntity subscriberEntity)
    : BusEntityStrategyBase<ChannelEntityContext>(subscriberEntity),
        IBusEntityCreationStrategy,
        IBusEntitySubscriptionStrategy,
        IBusEntityDisposeStrategy
{
    private readonly ChannelSubscriberEntity _subscriberEntity = subscriberEntity;
    private ISubscriber? _subscriber;

    private ILogger<ChannelSubscriberStrategy> Logger =>
        Context.LoggerFactory.CreateLogger<ChannelSubscriberStrategy>();
    
    [Description("Creating Channel Subscription")]
    public Task CreateEntity()
    {
        _subscriber = Context.ConnectionModule.GetSubscriber(_subscriberEntity.BusName);
        return Task.CompletedTask;
    }
    
    [Description("Subscribing to Channel")]
    public async Task SubscribeEntity()
    {
        if (_subscriber == null)
        {
            throw new NullReferenceException("Subscriber not initialized.");
        }
        
        await _subscriber.SubscribeAsync(_subscriberEntity.EntityName, (_, value) =>
        {
            byte[]? valueData = value;
            if (valueData is null or { Length: 0})
            {
                Logger.LogError("Message received on channel: {ChannelName} was null or had zero bytes.", 
                    _subscriberEntity.EntityName);
                return;
            }
            
            DispatchChannelMessage(valueData).GetAwaiter().GetResult();
        });
    }
    
    public async Task DispatchChannelMessage(byte[] value)
    {
        Type messageType = _subscriberEntity.Dispatcher.MessageType;
        var (contentType, messageData) = ChannelMessageEncoder.UnPack(value);
            
        var domainEvent = (IDomainEvent?)Context.Serialization.Deserialize(contentType, messageType, messageData);
        if (domainEvent == null)
        {
            Logger.LogError("The message received on channel: {ChannelName} could not be deserialized to type: " + 
                             "{DomainEventType}", _subscriberEntity.EntityName,  messageType);
            return;
        }
        
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, domainEvent);
        
        LogReceivedDomainEvent(domainEvent);
        AddMessageLogDetails(msgLog);

        try
        {
            // Invoke the in-process handler within new child scope:
            await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(_subscriberEntity.Dispatcher, domainEvent);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError("Channel Subscription", ex);
            throw;
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    // ---------- [Logging] ----------
    
    private void LogReceivedDomainEvent(IDomainEvent domainEvent)
    {
        var dispatchInfo = new
        {
            _subscriberEntity.Dispatcher.MessageType,
            _subscriberEntity.Dispatcher.ConsumerType, 
            _subscriberEntity.Dispatcher.MessageHandlerMethod.Name
        };

        var log = LogMessage.For(LogLevel.Information,
            "Domain event {EventName} Received on Redis Channel {ChannelName} on {DatabaseName}",
            domainEvent.GetType().Name,
            _subscriberEntity.EntityName,
            _subscriberEntity.BusName).WithProperties(
                LogProperty.ForName("DispatchInfo", dispatchInfo),
                LogProperty.ForName("DomainEvent", domainEvent)
        );
            
        Logger.Log(log);
    }
    
    private void AddMessageLogDetails(MessageLog msgLog)
    {
        if (! Context.MessageLogger.IsLoggingEnabled) return;
            
        msgLog.SentHint("subscribe-redis");
        msgLog.AddLogDetail("Database Name", _subscriberEntity.BusName);
        msgLog.AddLogDetail("ChannelName", _subscriberEntity.EntityName);
        msgLog.AddLogDetail("Message Type", _subscriberEntity.Dispatcher.MessageType.Name);
        msgLog.AddLogDetail("Handler Type", _subscriberEntity.Dispatcher.ConsumerType.FullName!);
        msgLog.AddLogDetail("Handler Method", _subscriberEntity.Dispatcher.MessageHandlerMethod.Name);
    }

    public async Task OnDispose()
    {
        if (_subscriber != null)
        {
            await _subscriber.UnsubscribeAllAsync();
        }
    }
}