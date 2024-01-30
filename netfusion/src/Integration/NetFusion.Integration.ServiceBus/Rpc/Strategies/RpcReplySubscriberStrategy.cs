using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.Bus.Rpc;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;

namespace NetFusion.Integration.ServiceBus.Rpc.Strategies;

/// <summary>
/// Creates an unique queue on which replies to RPC messages are sent to published messages.
/// Once a reply is received, the MessageId is used to correlate the message back to the
/// original published message's TaskCompletionSource on which the microservice is awaiting.
/// </summary>
internal class RpcReplySubscriberStrategy(RpcReferenceEntity rpcEntity)
    : BusEntityStrategyBase<NamespaceEntityContext>(rpcEntity),
        IBusEntityCreationStrategy,
        IBusEntitySubscriptionStrategy,
        IBusEntityDisposeStrategy
{
    private readonly RpcReferenceEntity _rpcEntity = rpcEntity;
    private ServiceBusProcessor? _replyProcessor;

    private ILogger<RpcReplySubscriberStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<RpcReplySubscriberStrategy>();

    public async Task CreateEntity()
    {
        var connection = Context.NamespaceModule.GetConnection(_rpcEntity.BusName);
        var options = new CreateQueueOptions(_rpcEntity.ReplyQueueName)
        {
            // In case the publishing microservice was not stopped cleanly, configure
            // the replay queue to be automatically deleted when all subscriptions no
            // longer exist.
            AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
        };

        await connection.CreateOrUpdateQueue(_rpcEntity.EntityName, options);
    }

    public Task SubscribeEntity()
    {
        var connection = Context.NamespaceModule.GetConnection(_rpcEntity.BusName);
        var processingOptions = _rpcEntity.ProcessingOptions;

        _replyProcessor = connection.BusClient.CreateProcessor(_rpcEntity.ReplyQueueName,
            new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = processingOptions.PrefetchCount,
                MaxConcurrentCalls = processingOptions.MaxConcurrentCalls,
            });
            
        _replyProcessor.ProcessMessageAsync += OnMessageReceived;
        _replyProcessor.ProcessErrorAsync += OnProcessingError;
            
        return _replyProcessor.StartProcessingAsync();
    }
    
    private Task OnMessageReceived(ProcessMessageEventArgs args)
    {
        string messageId = args.Message.MessageId;
                    
        if (string.IsNullOrWhiteSpace(messageId))
        {
            Logger.LogError("The received reply message does not have a Message Id.");
            return Task.CompletedTask;
        }

        Logger.LogDebug(
            "Received Response for Command with Message Id {MessageId}.", messageId);

        if (! _rpcEntity.PendingRequests.TryRemove(messageId, out RpcPendingRequest? pendingRequest))
        {
            Logger.LogError("The received Message Id: {MessageId} does not have pending request.", messageId);
            return Task.CompletedTask;       
        }
            
        Logger.LogDebug("Message received on RPC reply queue {QueueName} with Message Id {MessageId}",
            _rpcEntity.ReplyQueueName, messageId);

        SetReplyResult(pendingRequest, args);
        return Task.CompletedTask;
    }

    
    private static void SetReplyResult(RpcPendingRequest pendingRequest, ProcessMessageEventArgs args)
    {
        var replyEx = CheckReplyException(args);
        if (replyEx != null)
        {
            pendingRequest.SetException(replyEx);
            return;
        }
            
        pendingRequest.SetResult(args.Message.Body.ToArray());
    }
    
    private static RpcReplyException? CheckReplyException(ProcessMessageEventArgs args)
    {
        if (args.Message.ApplicationProperties.TryGetValue("RpcError", out object? value))
        {
            string replyException = value.ToString() ?? string.Empty;
                
            return string.IsNullOrEmpty(replyException)
                ? new RpcReplyException("RPC Queue subscriber indicated error without details.")
                : new RpcReplyException(replyException);
        }

        return null;
    }
    
    private Task OnProcessingError(ProcessErrorEventArgs args)
    {
        Context.LogProcessError(args);
        return Task.CompletedTask;
    }

    public async Task OnDispose()
    {
        if (_replyProcessor != null)
        {
            await _replyProcessor.DisposeAsync();
        }

        var connection = Context.NamespaceModule.GetConnection(_rpcEntity.BusName);
        await connection.AdminClient.DeleteQueueAsync(_rpcEntity.ReplyQueueName);
    }
}