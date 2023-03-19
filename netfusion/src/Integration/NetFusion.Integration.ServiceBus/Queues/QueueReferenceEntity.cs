using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Queues.Metadata;
using NetFusion.Integration.ServiceBus.Queues.Strategies;

namespace NetFusion.Integration.ServiceBus.Queues;

/// <summary>
/// Used by a microservice to define a reference to an existing queue,
/// created by another microservice, to which commands of a specific
/// type should be sent.
/// </summary>
internal class QueueReferenceEntity : BusEntity
{
    public Type CommandType { get; }

    /// <summary>
    /// Properties specifying how the command should be published.
    /// </summary>
    public QueuePublishOptions PublishOptions { get; } = new();
    
    public QueueReferenceEntity(Type commandType, string namespaceName, string queueName) 
        : base(namespaceName, queueName)
    {
        CommandType = commandType;

        AddStrategies(new QueuePublishStrategy(this));
    }
    
    /// <summary>
    /// The optional reply queue to be sent the response of the command. 
    /// </summary>
    public string? ReplyQueueName { get; set; }

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "CommandType", CommandType.Name },
        { "BusName", BusName},
        { "QueueName", EntityName },
        { "ReplyQueueName", ReplyQueueName },
    };
}