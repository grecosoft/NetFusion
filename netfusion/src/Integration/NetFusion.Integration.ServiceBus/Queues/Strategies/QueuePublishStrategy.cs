using System.ComponentModel;
using Azure.Messaging.ServiceBus;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.ServiceBus.Queues.Strategies;

/// <summary>
/// Strategy that publishes commands to a queue for processing
/// by another microservice defining the queue.
/// </summary>
internal class QueuePublishStrategy : BusEntityStrategyBase<NamespaceEntityContext>,
    IBusEntityCreationStrategy,
    IBusEntityPublishStrategy,
    IBusEntityDisposeStrategy
{
    private readonly QueueReferenceEntity _queueEntity;
    private ServiceBusSender? _serviceBusSender;

    public QueuePublishStrategy(QueueReferenceEntity queueEntity)
    {
        _queueEntity = queueEntity;
    }
    
    private ILogger<QueuePublishStrategy> Logger =>
        Context.LoggerFactory.CreateLogger<QueuePublishStrategy>();

    [Description("Creating Sender to publish Commands to Queue.")]
    public Task CreateEntity()
    {
        NamespaceConnection connection = Context.NamespaceModule.GetConnection(_queueEntity.BusName);
        _serviceBusSender = connection.BusClient.CreateSender(_queueEntity.EntityName, 
            new ServiceBusSenderOptions { Identifier = Context.HostPlugin.PluginId });
        
        return Task.CompletedTask;
    }

    public bool CanPublishMessageType(Type messageType) => messageType == _queueEntity.CommandType;

    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        if (_serviceBusSender == null)
        {
            throw new NullReferenceException("Service Bus sender not initialized.");
        }

        string contentType = message.GetContentType() ?? _queueEntity.PublishOptions.ContentType;
        ServiceBusMessage busMessage = Context.CreateServiceBusMessage(contentType, message);

        if (! string.IsNullOrWhiteSpace(_queueEntity.ReplyQueueName))
        {
            busMessage.ReplyTo = $"{_queueEntity.BusName}:{_queueEntity.ReplyQueueName}";
        }
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("service-bus-publish-queue");

        LogPublishedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            await _serviceBusSender.SendMessageAsync(busMessage, cancellationToken);
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

    public async Task OnDispose()
    {
        if (_serviceBusSender != null)
        {
            await _serviceBusSender.DisposeAsync();
        }
    }
}