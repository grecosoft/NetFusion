using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Queues.Strategies;

namespace NetFusion.Integration.RabbitMQ.Queues;

/// <summary>
/// Defines a reference to a queue, defined by another microservice, to
/// which the calling microservice wants to send commands for processing.
/// </summary>
public class QueueReferenceEntity : BusEntity
{
    public Type CommandType { get; }
    public string? ReplyQueueName { get; set; }

    /// <summary>
    /// Properties specifying how the command should be published.
    /// </summary>
    public PublishOptions PublishOptions { get; } = new();

    public QueueReferenceEntity(Type commandType, string namespaceName, string queueName) 
        : base(namespaceName, queueName)
    {
        CommandType = commandType;

        AddStrategies(new QueuePublishStrategy(this));
    }

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "CommandType", CommandType.Name },
        { "BusName", BusName },
        { "QueueName", EntityName },
        { "ReplyQueueName", ReplyQueueName },
        { "Priority", PublishOptions.Priority?.ToString() },
        { "IsMandatory", PublishOptions.IsMandatory.ToString() },
        { "IsPersistent", PublishOptions.IsPersistent.ToString() }
    };
}