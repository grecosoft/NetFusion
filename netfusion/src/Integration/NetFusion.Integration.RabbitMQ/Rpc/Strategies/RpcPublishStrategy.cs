using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Integration.Bus.Rpc;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Messaging.Logging;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ.Rpc.Strategies;

/// <summary>
/// Used by a microservice to publish a RPC command to another microservice.
/// </summary>
public class RpcPublishStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityPublishStrategy
{
    private readonly RpcReferenceEntity _rpcEntity;
    
    public RpcPublishStrategy(RpcReferenceEntity rpcEntity)
    {
        _rpcEntity = rpcEntity;
    }
    
    private ILogger<RpcPublishStrategy> Logger =>
        Context.LoggerFactory.CreateLogger<RpcPublishStrategy>();
    
    public string ContentType => _rpcEntity.PublishOptions.ContentType;
    public bool CanPublishMessageType(Type messageType) => _rpcEntity.CommandMessageNamespaces.ContainsKey(messageType);
    

    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        var busConn = Context.BusModule.GetConnection(_rpcEntity.BusName);
        string contentType = message.GetContentType() ?? _rpcEntity.PublishOptions.ContentType;
        int cancelRpcRequestAfterMs = _rpcEntity.PublishOptions.CancelRpcRequestAfterMs;
        
        byte[] messageBody = Context.Serialization.Serialize(message, contentType);
        MessageProperties messageProperties = GetMessageProperties(message, contentType);
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("rabbitmq-publish-queue");

        LogPublishedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            var futureResult = RecordSentMessage(messageProperties.MessageId, cancelRpcRequestAfterMs,
                cancellationToken);

            await busConn.AdvancedBus.PublishAsync(Exchange.Default,
                _rpcEntity.EntityName,
                false,
                messageProperties,
                messageBody, cancellationToken).ConfigureAwait(false);

            // Waits for the RpcReplySubscriberStrategy to receive message on the reply queue.
            byte[] resultBytes = await futureResult.Task;
            SetCommandResult(message, resultBytes);
        }
        catch (OperationCanceledException ex)
        {
            if (_rpcEntity.PendingRequests.TryRemove(messageProperties.MessageId,
                    out RpcPendingRequest? pendingRequest))
            {
                pendingRequest.UnRegister();
            }

            throw new RpcReplyException(
                $"The RPC request with the Message Id of: {messageProperties.MessageId} was canceled. " +
                $"The current timeout value is: {cancelRpcRequestAfterMs} ms.",
                ex);
        }
        catch (Exception ex)
        {
            if (_rpcEntity.PendingRequests.TryRemove(messageProperties.MessageId,
                    out RpcPendingRequest? pendingRequest))
            {
                pendingRequest.UnRegister();
            }
            
            msgLog.AddLogError(nameof(RpcPublishStrategy), ex);

            throw new RpcReplyException(
                $"The RPC request with the Message Id of: {messageProperties.MessageId} resulted in an exception.", ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private MessageProperties GetMessageProperties(IMessage message, string contextType)
    {
        var props = new MessageProperties
        {
            ContentType = contextType,
            AppId = Context.HostPlugin.PluginId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            DeliveryMode = 1,

            ReplyTo = _rpcEntity.MessageReplyTo,
            MessageId = Guid.NewGuid().ToString()
        };
        
        props.SetRpcActionNamespace(_rpcEntity.CommandMessageNamespaces[message.GetType()]);
        return props;
    }
    
    // Returns a task source associated with the Message Id of the sent RPC command.
    // When a response is received on the reply queue, the task source is either marked
    // as completed or with an error.  
    private TaskCompletionSource<byte[]> RecordSentMessage(string messageId,
        int cancelRpcRequestAfterMs,
        CancellationToken cancellationToken)
    {
        // Create a task that can be completed in the future when the result
        // is received in the reply queue. 
        var futureResult = new TaskCompletionSource<byte[]>();
        var rpcPendingRequest = new RpcPendingRequest(futureResult, cancelRpcRequestAfterMs, cancellationToken);

        _rpcEntity.PendingRequests[messageId] = rpcPendingRequest;
        return futureResult;
    }
    
    private void SetCommandResult(IMessage message, byte[] resultBytes)
    {
        // If a successful reply, deserialize the response message into the
        // result type associated with the command.
        var commandResult = (IMessageWithResult)message;

        var responseObj = Context.Serialization.Deserialize(ContentType,
            commandResult.DeclaredResultType,
            resultBytes);

        commandResult.SetResult(responseObj);
    }
    
    private void LogPublishedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Publishing {MessageType} to {Entity} on {Bus} with {RouteKey}",
            message.GetType(),
            _rpcEntity.EntityName,
            _rpcEntity.BusName).WithProperties(
            LogProperty.ForName("ExchangeEntity", _rpcEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }

    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _rpcEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
    }
}