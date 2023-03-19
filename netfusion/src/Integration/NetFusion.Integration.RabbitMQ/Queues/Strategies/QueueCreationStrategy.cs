using System.ComponentModel;
using NetFusion.Integration.RabbitMQ.Bus;
using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Logging;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Queues.Strategies;

/// <summary>
/// Creates a queue subscribed by a microservice to which other microservices
/// can send commands for processing.
/// </summary>
public class QueueCreationStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly QueueEntity _queueEntity;
    private IDisposable? _consumer;
    
    private ILogger<QueueCreationStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<QueueCreationStrategy>();

    public QueueCreationStrategy(QueueEntity queueEntity)
    {
        _queueEntity = queueEntity;
    }
    
    [Description("Creating Queue to which Commands can be published.")]
    public async Task CreateEntity()
    {
        if (! Context.IsAutoCreateEnabled) return;

        BusConnection busConn = Context.BusModule.GetConnection(_queueEntity.BusName);
        busConn.ExternalSettings.ApplyQueueSettings(_queueEntity.EntityName, _queueEntity.QueueMeta);
        
        await busConn.CreateQueueAsync(_queueEntity.QueueMeta);
    }

    [Description("Subscribing to Queue for dispatching recevied Commands.")]
    public Task SubscribeEntity()
    {
        // Dispose current consumer in case of reconnection:
        _consumer?.Dispose();
        
        BusConnection busConn = Context.BusModule.GetConnection(_queueEntity.BusName);
        var queue = new Queue(_queueEntity.QueueMeta.QueueName);

        _consumer = busConn.AdvancedBus.Consume(queue, (msgData, msgProps, _, cancellationToken) => 
            OnMessageReceived(msgData.ToArray(), msgProps, cancellationToken), 
            config =>
            {
                config.WithPrefetchCount(_queueEntity.QueueMeta.PrefetchCount);
                config.WithExclusive(_queueEntity.QueueMeta.IsExclusive);
                config.WithPriority(_queueEntity.QueueMeta.Priority);
            });

         return Task.CompletedTask;
    }

    private async Task OnMessageReceived(byte[] msgData, MessageProperties msgProps, 
        CancellationToken cancellationToken)
    {
        IMessage? message = CreateMessage(msgData, msgProps);
        if (message == null)
        {
            return;
        }
        
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, message);
        msgLog.SentHint("rabbitmq-work-queue");
        
        LogReceivedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            IMessage? response = (IMessage?)await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(
                _queueEntity.MessageDispatcher,
                message,
                cancellationToken);

            if (response != null)
            {
                await RespondToReplyQueue(msgProps, response);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error dispatching message {MessageType} received on queue {QueueName} defined on {Bus}.",
                _queueEntity.MessageDispatcher.MessageType.Name,
                _queueEntity.EntityName,
                _queueEntity.BusName);

            msgLog.AddLogError(nameof(QueueCreationStrategy), ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }

    private IMessage? CreateMessage(ReadOnlyMemory<byte> msgData, MessageProperties msgProps)
    {
        Type messageType = _queueEntity.MessageDispatcher.MessageType;
        var message = (IMessage?)Context.Serialization.Deserialize(msgProps.ContentType, messageType, msgData.ToArray());

        if (message == null)
        {
            Logger.LogError("Message Type {messageType} could not be deserialized.", messageType);
            return null;
        }

        Context.SetMessageProperties(msgProps, message);
        return message;
    }
    
    private async Task RespondToReplyQueue(MessageProperties msgProps, IMessage response)
    {
        if (!msgProps.TryParseReplyTo(out string? busName, out string? queueName))
        {
            return;
        }
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, response);
        msgLog.SentHint("rabbitmq-send-reply-queue");

        LogResponseMessage(response, msgProps.ReplyTo);
        AddResponseDetailsToLog(msgLog, msgProps.ReplyTo);
        
        // Serialize the response message using the content-type of the original received message and set the
        // MessageId/CorrelationId of the original message so the response can be matched with the sent command.
        var responseMessageProps = new MessageProperties
        {
            ContentType = msgProps.ContentType,
            CorrelationId = msgProps.CorrelationId,
            MessageId = msgProps.MessageId
        };
        
        BusConnection busConn = Context.BusModule.GetConnection(busName);
        try
        {
            byte[] messageBody = Context.Serialization.Serialize(response, responseMessageProps.ContentType);
            await busConn.AdvancedBus.PublishAsync(Exchange.Default, queueName, false,
                responseMessageProps,
                messageBody);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(QueueCreationStrategy), ex);
            
            throw new MessageDispatchException(
                $"Error sending command response to reply queue: {msgProps.ReplyTo}", ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private void LogReceivedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Received from {Queue} on {Bus}",
            message.GetType(),
            _queueEntity.EntityName,
            _queueEntity.BusName).WithProperties(
            LogProperty.ForName("QueueInfo", _queueEntity.GetLogProperties())
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
    
    private void LogResponseMessage(IMessage response, string replyQueue)
    {
        Logger.LogDebug("Response {ResponseType} being sent to reply queue {ReplyQueue}", 
            response.GetType(), 
            replyQueue);
    }
    
    private static void AddResponseDetailsToLog(MessageLog msgLog, string replyQueue)
    {
        msgLog.AddLogDetail("ReplyQueue", replyQueue);
    }

    public override IDictionary<string, string> GetLog() => new Dictionary<string, string>
    {
        { "CommandType", _queueEntity.MessageDispatcher.MessageType.Name },
        { "QueueName", _queueEntity.QueueMeta.QueueName! },
        { "IsDurable", _queueEntity.QueueMeta.IsDurable.ToString() },
        { "IsAutoDelete", _queueEntity.QueueMeta.IsAutoDelete.ToString() },
        { "IsExclusive", _queueEntity.QueueMeta.IsExclusive.ToString() },
        { "PrefetchCount", _queueEntity.QueueMeta.PrefetchCount.ToString() },
        { "Consumer", _queueEntity.MessageDispatcher.ConsumerType.Name },
        { "Handler", _queueEntity.MessageDispatcher.MessageHandlerMethod.Name }
    };

    public Task OnDispose()
    {
        _consumer?.Dispose();
        return Task.CompletedTask;
    }
}