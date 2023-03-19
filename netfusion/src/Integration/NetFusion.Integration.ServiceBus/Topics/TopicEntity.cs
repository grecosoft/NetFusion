using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Topics.Metadata;
using NetFusion.Integration.ServiceBus.Topics.Strategies;

namespace NetFusion.Integration.ServiceBus.Topics;

/// <summary>
/// Defines a topic provided by a microservice to which it publishes
/// domain-events to notify other microservices of changes.
/// </summary>
internal class TopicEntity : BusEntity
{
    public Type DomainEventType { get; }
    public TopicMeta TopicMeta { get; } 
    public TopicPublishOptions PublishOptions { get; } = new();
    
    public TopicEntity(Type domainEventType, string namespaceName, string topicName, TopicMeta topicMeta) 
        : base(namespaceName, topicName)
    {
        DomainEventType = domainEventType;
        TopicMeta = topicMeta;
        
        AddStrategies(new TopicCreationStrategy(this));
    }

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "DomainEventType", DomainEventType.Name },
        { "BusName", BusName },
        { "MaxSizeInMegabytes", TopicMeta.MaxSizeInMegabytes?.ToString() },
        { "MaxMessageSizeInKilobytes", TopicMeta.MaxMessageSizeInKilobytes?.ToString() },
        { "RequiresDuplicateDetection", TopicMeta.RequiresDuplicateDetection?.ToString() },
        { "DefaultMessageTimeToLive", TopicMeta.DefaultMessageTimeToLive?.ToString() },
        { "AutoDeleteOnIdle", TopicMeta.AutoDeleteOnIdle?.ToString() },
        { "DuplicateDetectionHistoryTimeWindow", TopicMeta.DuplicateDetectionHistoryTimeWindow?.ToString() },
        { "SupportOrdering", TopicMeta.SupportOrdering?.ToString() },
        { "EnableBatchedOperations", TopicMeta.EnableBatchedOperations?.ToString() },
        { "EnablePartitioning", TopicMeta.EnablePartitioning?.ToString() }
    };
}