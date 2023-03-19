using System.Diagnostics.CodeAnalysis;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Rpc.Metadata;
using NetFusion.Integration.RabbitMQ.Rpc.Strategies;

namespace NetFusion.Integration.RabbitMQ.Rpc;

public class RpcEntity : BusEntity
{
    public RpcQueueMeta RpcQueueMeta { get; }
    
    // Since multiple RPC commands can be sent over the same queue, the following maps
    // a given command's message-namespace to the message dispatcher of the consumer.
    private readonly Dictionary<string, MessageDispatcher> _dispatchers = new();

    public RpcEntity(string busName, string queueName, RpcQueueMeta rpcQueueMeta) 
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
            throw new RabbitMqPluginException(
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
            { "RpcQueueName", RpcQueueMeta.QueueName },
            { "PrefetchCount", RpcQueueMeta.PrefetchCount.ToString() }
        };

        foreach (var (cmdNamespace, dispatcher) in _dispatchers)
        {
            props[cmdNamespace] = dispatcher.ToString();
        }

        return props;
    }
}