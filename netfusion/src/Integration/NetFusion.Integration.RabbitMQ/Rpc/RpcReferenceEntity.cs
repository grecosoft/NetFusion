using System.Collections.Concurrent;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Rpc;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Rpc.Metadata;
using NetFusion.Integration.RabbitMQ.Rpc.Strategies;

namespace NetFusion.Integration.RabbitMQ.Rpc;

public class RpcReferenceEntity : BusEntity
{
    // The name of the reply queue the consumer should send the command's response.
    public string ReplyQueueName { get; }
    
    // Value sent on the message, containing the Namespace and QueueName, used by the
    // consumer to resolve the ReplyQueue.
    public string MessageReplyTo { get; }

    /// <summary>
    /// Options used when processing the RPC reply queue.
    /// </summary>
    public RpcPublishOptions PublishOptions { get; } = new();
    
    // Multiple RPC commands can be sent on the same queue.  The following maps a command 
    // to its associated message-namespace.  This is used by the microservice providing
    // the RPC queue to determine how a command is routed to its consumer.
    public Dictionary<Type, string> CommandMessageNamespaces { get; } = new();

    // current pending sent RPC command for which the microservice is awaiting responses.
    public ConcurrentDictionary<string, RpcPendingRequest> PendingRequests { get; }
    
    public RpcReferenceEntity(string busName, string queueName) 
        : base(busName, queueName)
    {
        ReplyQueueName = $"rpc_{queueName}_{Guid.NewGuid()}";
        MessageReplyTo = $"{BusName}:{ReplyQueueName}";
        PendingRequests = new ConcurrentDictionary<string, RpcPendingRequest>();
        
        AddStrategies(
            new RpcPublishStrategy(this),
            new RpcReplySubscriberStrategy(this));
    }
    
    /// <summary>
    /// Specifies the message-namespace used to identify the command.
    /// </summary>
    /// <param name="commandType">The type of the command.</param>
    /// <param name="messageNamespace">The namespace used to identify the command.</param>
    public void AddCommandMessageNamespace(Type commandType, string messageNamespace)
    {
        if (CommandMessageNamespaces.TryGetValue(commandType, out string? existingMessageNamespace))
        {
            throw new RabbitMqPluginException(
                $"The command of type {commandType} is already associated with RPC reply queue {ReplyQueueName} " + 
                $"for the message-namespace {existingMessageNamespace}");
        }

        var existingCommand = CommandMessageNamespaces.Where(mn => mn.Value == messageNamespace)
            .Select(mn => mn.Key).FirstOrDefault();

        if (existingCommand != null)
        {
            throw new RabbitMqPluginException(
                $"The message-namespace {messageNamespace} is already associated with command of type {existingCommand}"); 
        }

        CommandMessageNamespaces[commandType] = messageNamespace;
    }

    protected override IDictionary<string, string?> OnLogProperties()
    {
        var props = new Dictionary<string, string?>
        {
            { "BusName", BusName },
            { "ReplyQueueName", ReplyQueueName }
        };

        foreach (var (cmdType, cmdNamespace) in CommandMessageNamespaces)
        {
            props[cmdNamespace] = cmdType.Name;
        }

        return props;
    }
}