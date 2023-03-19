using System.Diagnostics.CodeAnalysis;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Internal;
using NetFusion.Integration.ServiceBus.Rpc.Metadata;
using NetFusion.Integration.ServiceBus.Rpc.Strategies;

namespace NetFusion.Integration.ServiceBus.Rpc;

/// <summary>
/// Defines a queue, provided by a microservice, used to receive RPC style of messages from
/// other microservices.  Multiple commands can be sent over the same queue, identified by
/// a message-namespace, to efficiently use queues. 
/// </summary>
internal class RpcEntity : BusEntity
{
    public IRpcQueueMeta RpcQueueMeta { get; }
    
    // Since multiple RPC commands can be sent over the same queue, the following maps
    // a given command's message-namespace to the message dispatcher of the consumer.
    private readonly Dictionary<string, MessageDispatcher> _dispatchers = new();

    public RpcEntity(string busName, string queueName, IRpcQueueMeta rpcQueueMeta) 
        : base(busName, queueName)
    {
        RpcQueueMeta = rpcQueueMeta;
        
        AddStrategies(new RpcConsumerStrategy(this));
    }
    
    public override IEnumerable<MessageDispatcher> Dispatchers => _dispatchers.Values;

    public void AddMessageDispatcher(string messageNameSpace, MessageDispatcher dispatcher)
    {
        if (string.IsNullOrWhiteSpace(messageNameSpace))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(messageNameSpace));

        if (_dispatchers.ContainsKey(messageNameSpace))
        {
            throw new ServiceBusPluginException(
                $"The RPC Message Namespace of {messageNameSpace} already exists.", 
                "RPC_MSG_NS_ALREADY_EXISTS");
        }

        _dispatchers[messageNameSpace] = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public bool TryGetMessageDispatcher(string messageNamespace, [NotNullWhen(true)] out MessageDispatcher? dispatcher)
    {
        if (string.IsNullOrWhiteSpace(messageNamespace))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(messageNamespace));
        
        return _dispatchers.TryGetValue(messageNamespace, out dispatcher);
    }

    protected override IDictionary<string, string?> OnLogProperties()
    {
        var props = new Dictionary<string, string?>
        {
            { "BusName", BusName },
            { "RpcQueueName", EntityName },
            { "LockDuration", RpcQueueMeta.LockDuration?.ToString() },
            { "MaxDeliveryCount", RpcQueueMeta.MaxDeliveryCount?.ToString() },
            { "DefaultMessageTimeToLive", RpcQueueMeta.DefaultMessageTimeToLive?.ToString() },
            { "MaxSizeInMegabytes", RpcQueueMeta.MaxSizeInMegabytes?.ToString() }
        };

        foreach (var (ns, dispatcher) in _dispatchers)
        {
            props[ns] = dispatcher.ToString();
        }

        return props;
    }
}