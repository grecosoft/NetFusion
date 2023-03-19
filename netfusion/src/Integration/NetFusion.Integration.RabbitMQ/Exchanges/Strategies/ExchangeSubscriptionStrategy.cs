using System.ComponentModel;
using EasyNetQ.Topology;
using EasyNetQ;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Messaging.Logging;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Exchanges.Strategies;

/// <summary>
/// Creates a queue, subscribed to an exchange, on which microservices
/// receive domain events published by other microservices.
/// </summary>
public class ExchangeSubscriptionStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly SubscriptionEntity _subscriptionEntity;
    
    private IDisposable? _consumer;

    public ExchangeSubscriptionStrategy(SubscriptionEntity subscriptionEntity)
    {
        _subscriptionEntity = subscriptionEntity;
    }

    private ILogger<ExchangeSubscriptionStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<ExchangeSubscriptionStrategy>();

    [Description("Creating Queue bound to exchange for received Domain-Events.")]
    public async Task CreateEntity()
    {
        if (! Context.IsAutoCreateEnabled) return;
        
        BusConnection busConn = Context.BusModule.GetConnection(_subscriptionEntity.BusName);
        busConn.ExternalSettings.ApplyQueueSettings(_subscriptionEntity.EntityName, _subscriptionEntity.QueueMeta);
        
        if (_subscriptionEntity.IsPerServiceInstance)
        {
            // Each microservice instance will have its own queue.  The default is having a single queue
            // to which all microservices subscribe to which messages are delivered round robin. 
            _subscriptionEntity.QueueMeta.QueueName += $"_{Guid.NewGuid()}";
        }
        
        await busConn.CreateQueueAsync(_subscriptionEntity.QueueMeta);

        await busConn.BindQueueToExchange(_subscriptionEntity.QueueMeta.QueueName!, 
            _subscriptionEntity.ExchangeName,
            _subscriptionEntity.RouteKeys);
    }

    [Description("Subscribing to Queue bound Exchange for despatching received Domain-Events.")]
    public Task SubscribeEntity()
    {
        // Dispose current consumer in case of reconnection:
        _consumer?.Dispose();

        BusConnection busConn = Context.BusModule.GetConnection(_subscriptionEntity.BusName);
        var queue = new Queue(_subscriptionEntity.QueueMeta.QueueName);

        _consumer = busConn.AdvancedBus.Consume(queue,
            (msgData, msgProps, _, cancellationToken) => 
                OnMessageReceived(msgData.ToArray(), msgProps, cancellationToken), 
            config =>
            {
                config.WithPrefetchCount(_subscriptionEntity.QueueMeta.PrefetchCount);
                config.WithExclusive(_subscriptionEntity.QueueMeta.IsExclusive);
                config.WithPriority(_subscriptionEntity.QueueMeta.Priority);
            });

        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(byte[] msgData, MessageProperties msgProps, CancellationToken cancellationToken)
    {
        IMessage? message = CreateMessage(msgData, msgProps);
        if (message == null)
        {
            return;
        }
        
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, message);
        msgLog.SentHint("rabbitmq-exchange-queue");

        LogReceivedMessage(message);
        AddEntityDetailsToLog(msgLog);
        
        try
        {
            await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(
                _subscriptionEntity.MessageDispatcher,
                message,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error dispatching message {MessageType} received on queue {QueueName} defined on {Bus}.", 
                _subscriptionEntity.MessageDispatcher.MessageType.Name,
                _subscriptionEntity.EntityName,
                _subscriptionEntity.BusName);
            
            msgLog.AddLogError(nameof(ExchangeSubscriptionStrategy), ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private IMessage? CreateMessage(ReadOnlyMemory<byte> msgData, MessageProperties msgProps)
    {
        Type messageType = _subscriptionEntity.MessageDispatcher.MessageType;
        var message = (IMessage?)Context.Serialization.Deserialize(msgProps.ContentType, messageType, msgData.ToArray());

        if (message == null)
        {
            Logger.LogError("Message Type {messageType} could not be deserialized.", messageType);
            return null;
        }

        Context.SetMessageProperties(msgProps, message);
        return message;
    }
    
    private void LogReceivedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Received from {Queue} on {Bus}",
            message.GetType(),
            _subscriptionEntity.EntityName,
            _subscriptionEntity.BusName).WithProperties(
            LogProperty.ForName("QueueInfo", _subscriptionEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }

    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _subscriptionEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
    }
    
    public Task OnDispose()
    {
        _consumer?.Dispose();
        return Task.CompletedTask;
    }
}