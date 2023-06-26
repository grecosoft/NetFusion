using System.ComponentModel;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Messaging.Logging;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Exchanges.Strategies;

/// <summary>
/// Creates an exchange used to publish domain-events to which other microservices
/// can subscribe by binding a queue to the exchange.
/// </summary>
public class ExchangeCreationStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityCreationStrategy,
    IBusEntityPublishStrategy
{
    private readonly ExchangeEntity _exchangeEntity;
    private readonly ExchangeMeta _exchangeMeta;

    public ExchangeCreationStrategy(ExchangeEntity exchangeEntity) : base(exchangeEntity)
    {
        _exchangeEntity = exchangeEntity;
        _exchangeMeta = _exchangeEntity.ExchangeMeta;
    }

    private ILogger<ExchangeCreationStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<ExchangeCreationStrategy>();

    [Description("Creating Exchange to which Domain-Events can be published.")]
    public async Task CreateEntity()
    {
        if (! Context.IsAutoCreateEnabled) return;
        
        IBusConnection busConn = Context.BusModule.GetConnection(_exchangeEntity.BusName);
        busConn.ExternalSettings.ApplyExchangeSettings(_exchangeEntity.EntityName, _exchangeMeta);
        
        await busConn.CreateExchangeAsync(_exchangeMeta);
        
        if (! string.IsNullOrEmpty(_exchangeMeta.AlternateExchangeName))
        {
            await busConn.CreateAlternateExchange(_exchangeMeta.AlternateExchangeName);
        }
    }

    public bool CanPublishMessageType(Type messageType) => messageType == _exchangeEntity.DomainEventType;

    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        if (!_exchangeEntity.ExchangeMeta.WhenDomainEvent(message))
        {
            return;
        }
        
        string contentType = message.GetContentType() ?? _exchangeMeta.PublishOptions.ContentType;
        byte[] messageBody = Context.Serialization.Serialize(message, contentType);
        
        var messageProperties = Context.GetMessageProperties(message);
        messageProperties.SetPublishOptions(contentType, _exchangeMeta.PublishOptions);

        string routeKey = message.GetRouteKey() ?? _exchangeMeta.GetRouteKey(message);
        
        var busConn = Context.BusModule.GetConnection(_exchangeEntity.BusName);
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("rabbitmq-publish-exchange");

        LogPublishedMessage(message, routeKey);
        AddEntityDetailsToLog(msgLog, routeKey);
            
        try
        {
            await busConn.PublishToExchange(_exchangeMeta.ExchangeName,
                routeKey,
                _exchangeMeta.PublishOptions.IsMandatory,
                messageProperties,
                messageBody, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(ExchangeCreationStrategy), ex);
            throw;
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private void LogPublishedMessage(IMessage message, string routeKey)
    {
        var log = LogMessage.For(LogLevel.Debug, "Publishing {MessageType} to {Entity} on {Bus} with {RouteKey}",
            message.GetType(),
            _exchangeEntity.EntityName,
            _exchangeEntity.BusName,
            routeKey).WithProperties(
            LogProperty.ForName("ExchangeEntity", _exchangeEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }
    
    private void AddEntityDetailsToLog(MessageLog msgLog, string routeKey)
    {
        foreach ((string key, string? value) in _exchangeEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
        
        msgLog.AddLogDetail("RouteKey", routeKey);
    }
}