using EasyNetQ;
using EasyNetQ.Topology;
using NetFusion.Integration.Bus.Rpc;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;

namespace NetFusion.Integration.RabbitMQ.Rpc.Strategies;

/// <summary>
/// Strategy used by a microservice sending a RPC command to monitor the RPC
/// reply queue for a response that is correlated back to the original sent
/// command.
/// </summary>
public class RpcReplySubscriberStrategy : BusEntityStrategyBase<EntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly RpcReferenceEntity _rpcEntity;
    
    private Queue _queue;
    private IDisposable? _consumer;
    
    private ILogger<RpcReplySubscriberStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<RpcReplySubscriberStrategy>();

    public RpcReplySubscriberStrategy(RpcReferenceEntity rpcEntity) : base(rpcEntity)
    {
        _rpcEntity = rpcEntity;
    }
    
    public async Task CreateEntity()
    {
        IBusConnection busConn = Context.BusModule.GetConnection(_rpcEntity.BusName);
  
        _queue = await busConn.CreateQueueAsync(new QueueMeta
        {
            QueueName = _rpcEntity.ReplyQueueName,
            IsDurable = false,
            IsExclusive = true,
            IsAutoDelete = true
        });
    }

    public Task SubscribeEntity()
    {
        // Dispose current consumer in case of reconnection:
        _consumer?.Dispose();
        
        IBusConnection busConn = Context.BusModule.GetConnection(_rpcEntity.BusName);

        _consumer = busConn.ConsumeRpcReplyQueue(_rpcEntity, OnMessageReceived);
        return Task.CompletedTask;
    }

    private void OnMessageReceived(byte[] msgData, MessageProperties msgProps)
    {
        string messageId = msgProps.MessageId;
                
        if (string.IsNullOrWhiteSpace(messageId))
        {
            Logger.LogError("The received reply message does not have a Message Id.");
            return;
        }
                
        Logger.LogDebug("Received Response for Command with Message Id {MessageId}.", messageId);
                
        if (! _rpcEntity.PendingRequests.TryRemove(messageId, out RpcPendingRequest? pendingRequest))
        {
            Logger.LogError("The received Message Id: {MessageId} does not have pending request.", messageId);
            return;
        }
                
        Logger.LogDebug("Message received on RPC reply queue {QueueName} with Message Id {MessageId}",
            _rpcEntity.ReplyQueueName, messageId);
                
        SetReplyResult(pendingRequest, msgData, msgProps);
    }
    
    private static void SetReplyResult(RpcPendingRequest pendingRequest, byte[] msgData, MessageProperties msgProps)
    {
        var replyEx = CheckReplyException(msgProps);
        if (replyEx != null)
        {
            pendingRequest.SetException(replyEx);
            return;
        }
            
        pendingRequest.SetResult(msgData);
    }
    
    private static RpcReplyException? CheckReplyException(MessageProperties msgProps)
    {
        string? replyException = msgProps.GetRpcReplyException();
        if (replyException == null)
        {
            return null;
        }
        
        return replyException == string.Empty
            ? new RpcReplyException("RPC Queue subscriber indicated error without details.")
            : new RpcReplyException(replyException);
    }

    public Task OnDispose()
    {
        _consumer?.Dispose();
    
        foreach (RpcPendingRequest pendingRequest in _rpcEntity.PendingRequests.Values)
        {
            pendingRequest.Cancel();
        }

        return Task.CompletedTask;
    }
}