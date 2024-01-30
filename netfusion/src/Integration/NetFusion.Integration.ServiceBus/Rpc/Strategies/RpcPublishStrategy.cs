using Azure.Messaging.ServiceBus;
using NetFusion.Integration.Bus.Rpc;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.ServiceBus.Rpc.Strategies;

/// <summary>
/// Allows a microservice to publish RPC messages to another microservice's RPC queue.
/// Each published message has a message-namespace used by the consuming microservice
/// to identify the type of message.  Once the message is published, the microservice
/// awaits a response on it corresponding reply queue.  If a reply is not received
/// within a specified amount of time, the request is canceled.
/// </summary>
internal class RpcPublishStrategy(RpcReferenceEntity rpcEntity)
    : BusEntityStrategyBase<NamespaceEntityContext>(rpcEntity),
        IBusEntityCreationStrategy,
        IBusEntityPublishStrategy,
        IBusEntityDisposeStrategy
{
    private readonly RpcReferenceEntity _rpcEntity = rpcEntity;
    private ServiceBusSender? _rpcQueueSender;

    private ILogger<RpcPublishStrategy> Logger =>
        Context.LoggerFactory.CreateLogger<RpcPublishStrategy>();
    
    public bool CanPublishMessageType(Type messageType) => _rpcEntity.CommandMessageNamespaces.ContainsKey(messageType);

    public Task CreateEntity()
    {
        NamespaceConnection connection = Context.NamespaceModule.GetConnection(_rpcEntity.BusName);
        
        _rpcQueueSender = connection.BusClient.CreateSender(_rpcEntity.EntityName, 
            new ServiceBusSenderOptions{ Identifier = Context.HostPlugin.PluginId });

        return Task.CompletedTask;
    }
    
    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        if (_rpcQueueSender == null)
        {
            throw new NullReferenceException("Message can't be published until sender is created.");
        }
        
        int cancelRpcRequestAfterMs = _rpcEntity.PublishOptions.CancelRpcRequestAfterMs;
        
        string contentType = message.GetContentType() ?? _rpcEntity.PublishOptions.ContentType;
        ServiceBusMessage busMessage = Context.CreateServiceBusMessage(contentType, message);
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("service-bus-publish-queue");

        LogPublishedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            SetRpcMessageProperties(message, busMessage);

            var futureResult = RecordSentMessage(busMessage, cancelRpcRequestAfterMs, cancellationToken);
            await _rpcQueueSender.SendMessageAsync(busMessage, cancellationToken);

            // Waits for the RpcReplySubscriberStrategy to receive message on the reply queue.
            byte[] resultBytes = await futureResult.Task;
            SetCommandResult(message, resultBytes);

        }
        catch (OperationCanceledException ex)
        {
            if (_rpcEntity.PendingRequests.TryRemove(busMessage.MessageId,
                    out RpcPendingRequest? pendingRequest))
            {
                pendingRequest.UnRegister();
            }

            throw new RpcReplyException(
                $"The RPC request with the Message Id of: {busMessage.MessageId} was canceled. " +
                $"The current timeout value is: {cancelRpcRequestAfterMs} ms.",
                ex);
        }
        catch (Exception ex)
        {
            if (_rpcEntity.PendingRequests.TryRemove(busMessage.MessageId,
                    out RpcPendingRequest? pendingRequest))
            {
                pendingRequest.UnRegister();
            }
            
            msgLog.AddLogError(nameof(RpcPublishStrategy), ex);

            throw new RpcReplyException(
                $"The RPC request with the Message Id of: {busMessage.MessageId} resulted in an exception.", ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }

    private void SetRpcMessageProperties(IMessage message, ServiceBusMessage busMessage)
    {
        busMessage.ReplyTo = _rpcEntity.MessageReplyTo;
        busMessage.ApplicationProperties["MessageNamespace"] = _rpcEntity.CommandMessageNamespaces[message.GetType()];
        
        // Since the publisher of a RPC message will timeout after a specified number of milliseconds,
        // the message's TimeToLive will be set to the same value.  This way the message will not remain
        // within the queue, since after the timeout, the publisher will have deleted the record of sending
        // the message.
        busMessage.TimeToLive = TimeSpan.FromMilliseconds(_rpcEntity.PublishOptions.CancelRpcRequestAfterMs);
    }

    // Returns a task source associated with the Message Id of the sent RPC command.
    // When a response is received on the reply queue, the task source is either marked
    // as completed or with an error.  
    private TaskCompletionSource<byte[]> RecordSentMessage(ServiceBusMessage busMessage,
        int cancelRpcRequestAfterMs,
        CancellationToken cancellationToken)
    {
        // Create a task that can be completed in the future when the result
        // is received in the reply queue. 
        var futureResult = new TaskCompletionSource<byte[]>();
        var rpcPendingRequest = new RpcPendingRequest(futureResult, cancelRpcRequestAfterMs, cancellationToken);

        _rpcEntity.PendingRequests[busMessage.MessageId] = rpcPendingRequest;
        return futureResult;
    }
    
    private void SetCommandResult(IMessage message, byte[] resultBytes)
    {
        // If a successful reply, deserialize the response message into the
        // result type associated with the command.
        var commandResult = (IMessageWithResult)message;

        var responseObj = Context.Serialization.Deserialize(_rpcEntity.PublishOptions.ContentType,
            commandResult.DeclaredResultType,
            resultBytes);

        commandResult.SetResult(responseObj);
    }

    public async Task OnDispose()
    {
        if (_rpcQueueSender != null)
        {
            await _rpcQueueSender.DisposeAsync();
        }
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