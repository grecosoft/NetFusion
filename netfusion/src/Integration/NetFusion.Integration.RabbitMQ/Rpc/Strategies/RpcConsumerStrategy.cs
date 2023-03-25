using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Messaging.Logging;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Rpc.Strategies;

/// <summary>
/// Creates a queue on which a microservice can receive RPC messages.
/// To allow efficient use of queues, multiple messages can be sent
/// on the same queue identified by a message-namespace.
/// </summary>
public class RpcConsumerStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly RpcEntity _rpcEntity;

    private Queue _queue;
    private IDisposable? _consumer;
    
    public RpcConsumerStrategy(RpcEntity rpcEntity) : base(rpcEntity)
    {
        _rpcEntity = rpcEntity;
    }
    
    private ILogger<RpcConsumerStrategy> Logger => Context.LoggerFactory.CreateLogger<RpcConsumerStrategy>();

    public async Task CreateEntity()
    {
        BusConnection busConn = Context.BusModule.GetConnection(_rpcEntity.BusName);
        var queueMeta = new QueueMeta
        {
            QueueName = _rpcEntity.EntityName,
            IsAutoDelete = true,
            IsDurable = false,
            IsExclusive = false
        };
        
        _queue = await busConn.CreateQueueAsync(queueMeta);
    }

    public Task SubscribeEntity()
    {
        // Dispose current consumer in case of reconnection:
        _consumer?.Dispose();
        
        BusConnection busConn = Context.BusModule.GetConnection(_rpcEntity.BusName);

        _consumer = busConn.AdvancedBus.Consume(_queue, (msgData, msgProps, _, cancellationToken) =>
                OnMessageReceived(msgData.ToArray(), msgProps, cancellationToken),
            config =>
            {
                config.WithPrefetchCount(_rpcEntity.RpcQueueMeta.PrefetchCount);
                config.WithExclusive(false);
            });

        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(byte[] msgData, MessageProperties msgProps, 
        CancellationToken cancellationToken)
    {
        var messageDispatcher = GetMessageDispatcher(msgProps);
        if (messageDispatcher == null)
        {
            await ReplyWithError(msgProps, "The message was received but could not be dispatched using provided namespace.");
            return;
        }
        
        IMessage? message = CreateMessage(messageDispatcher.MessageType, msgData, msgProps);
        if (message == null)
        {
            await ReplyWithError(msgProps, "The message was received but could not be deserialized.");
            return;
        }

        await DispatchMessage(messageDispatcher, message, msgProps, cancellationToken);
    }

    private async Task DispatchMessage(MessageDispatcher messageDispatcher, IMessage message,
        MessageProperties msgProps,
        CancellationToken cancellationToken)
    {
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, message);
        msgLog.SentHint("rabbitmq-rpc-queue");
        
        LogReceivedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            object? response = await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(
                messageDispatcher,
                message,
                cancellationToken);

            if (response == null)
            {
                Logger.LogWarning("{MessageDispatcher} returned null response.", messageDispatcher.ToString());

                await ReplyWithError(msgProps,
                    "A null response message was returned in response to RPC request.");
                return;
            }
            
            await PublishReply(msgProps, response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error dispatching RPC message {MessageType} received on queue {QueueName} defined on {Bus}.",
                message.GetType().Name,
                _rpcEntity.EntityName,
                _rpcEntity.BusName);

            msgLog.AddLogError(nameof(RpcConsumerStrategy), ex);
            await ReplyWithError(msgProps, ex.ToString());
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private MessageDispatcher? GetMessageDispatcher(MessageProperties msgProps)
    {
        string? messageNs = msgProps.GetRpcActionNamespace();
        if (messageNs == null)
        {
            Logger.LogDebug("Received RPC message didn't have message-namespace specified.");
            return null;
        }
        
        if (! _rpcEntity.TryGetMessageDispatcher(messageNs, out MessageDispatcher? dispatcher))
        {
            Logger.LogDebug(
                "A message dispatcher for {MessageNamespace} delivered to {Queue} on {Bus} is not configured",
                messageNs, _rpcEntity.EntityName, _rpcEntity.BusName);
        }

        return dispatcher;
    }
    
    private IMessage? CreateMessage(Type messageType, byte[] msgData, MessageProperties msgProps)
    {
        var message = (IMessage?)Context.Serialization.Deserialize(msgProps.ContentType, messageType, msgData);
        if (message == null)
        {
            Logger.LogError("RPC message Type {messageType} could not be deserialized.", messageType);
            return null;
        }

        Context.SetMessageProperties(msgProps, message);
        return message;
    }

    private Task ReplyWithError(MessageProperties msgProps, string errorMessage)
    {
        msgProps.SetRpcReplyException(errorMessage);
        return PublishReply(msgProps);
    }

    private async Task PublishReply(MessageProperties msgProps, object? response = null)
    {
        if (!msgProps.TryParseReplyTo(out string? busName, out string? queueName))
        {
            throw new InvalidOperationException(
                $"The ReplyTo message property of: {msgProps.ReplyTo} does not specify " +
                "the name of the message bus and queue joined by a : character.");
        }
        
        var replyMsgProps = new MessageProperties
        {
            ContentType = msgProps.ContentType,
            MessageId = msgProps.MessageId
        };
        
        byte[] messageBody = response != null ? 
            Context.Serialization.Serialize(response, msgProps.ContentType) : 
            Array.Empty<byte>();

        var msgLog = new MessageLog(LogContextType.ReceivedMessage, response);
        msgLog.SentHint("rabbitmq-rpc-response");
        
        if (response != null)
        {
            LogResponseMessage(response, msgProps.ReplyTo);
        }
        
        BusConnection busCon = Context.BusModule.GetConnection(busName);
        try
        {
            await busCon.AdvancedBus.PublishAsync(Exchange.Default, queueName, 
                false,
                replyMsgProps,
                messageBody);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(RpcConsumerStrategy), ex);
            throw;
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
            _rpcEntity.EntityName,
            _rpcEntity.BusName).WithProperties(
            LogProperty.ForName("QueueInfo", _rpcEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }
    
    private void LogResponseMessage(object response, string replyQueue)
    {
        Logger.LogDebug("Response {ResponseType} being sent to reply queue {ReplyQueue}", 
            response.GetType(), 
            replyQueue);
    }

    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _rpcEntity.GetLogProperties())
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