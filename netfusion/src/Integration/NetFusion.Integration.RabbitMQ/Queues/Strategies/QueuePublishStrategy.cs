using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Messaging.Logging;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Queues.Strategies;

/// <summary>
/// Strategy used by a microservice to send commands to a queue provided
/// by another microservice.
/// </summary>
public class QueuePublishStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityPublishStrategy
{
    private readonly QueueReferenceEntity _queueEntity;

    public QueuePublishStrategy(QueueReferenceEntity queueEntity)
    {
        _queueEntity = queueEntity;
    }

    private ILogger<QueuePublishStrategy> Logger =>
        Context.LoggerFactory.CreateLogger<QueuePublishStrategy>();
    
    public bool CanPublishMessageType(Type messageType) => messageType == _queueEntity.CommandType;

    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        var busConn = Context.BusModule.GetConnection(_queueEntity.BusName);
        busConn.ExternalSettings.ApplyPublishSettings(_queueEntity.EntityName, _queueEntity.PublishOptions);
        
        string contentType = message.GetContentType() ?? _queueEntity.PublishOptions.ContentType;
        byte[] messageBody = Context.Serialization.Serialize(message, contentType);

        var messageProperties = Context.GetMessageProperties(message);
        messageProperties.SetPublishOptions(contentType, _queueEntity.PublishOptions);
        SetOptionalReplyToProperty(messageProperties);

        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("rabbitmq-publish-queue");

        LogPublishedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            await busConn.AdvancedBus.PublishAsync(Exchange.Default,
                _queueEntity.EntityName,
                _queueEntity.PublishOptions.IsMandatory,
                messageProperties,
                messageBody, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(QueuePublishStrategy), ex);
            throw;
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private void SetOptionalReplyToProperty(MessageProperties msgProps)
    {
        var replyTo = _queueEntity.ReplyQueueName;
        if (replyTo != null)
        {
            msgProps.ReplyTo =  $"{_queueEntity.BusName}:{replyTo}";
        }
    }

    private void LogPublishedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Publishing {MessageType} to {Entity} on {Bus} with {RouteKey}",
            message.GetType(),
            _queueEntity.EntityName,
            _queueEntity.BusName).WithProperties(
            LogProperty.ForName("ExchangeEntity", _queueEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }

    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _queueEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
    }
}