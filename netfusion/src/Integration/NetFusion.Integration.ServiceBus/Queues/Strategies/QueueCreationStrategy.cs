using System.ComponentModel;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Queues.Metadata;

namespace NetFusion.Integration.ServiceBus.Queues.Strategies;

/// <summary>
/// Strategy that creates a queue to with commands can be sent from other
/// microservices for processing.
/// </summary>
internal class QueueCreationStrategy(QueueEntity queueEntity)
    : BusEntityStrategyBase<NamespaceEntityContext>(queueEntity),
        IBusEntityCreationStrategy
{
    private readonly QueueEntity _queueEntity = queueEntity;

    [Description("Creating Queue to which Commands can be sent for processing.")]
    public Task CreateEntity()
    {
        if (!Context.IsAutoCreateEnabled) return Task.CompletedTask;
        
        NamespaceConnection connection = Context.NamespaceModule.GetConnection(_queueEntity.BusName);
        CreateQueueOptions queueOptions = BuildCreateOptions(_queueEntity.EntityName, _queueEntity.QueueMeta);
        return connection.CreateOrUpdateQueue(_queueEntity.EntityName, queueOptions);
    }

    private static CreateQueueOptions BuildCreateOptions(string queueName, QueueRouteMeta queueMeta)
    {
        var defaults = new CreateQueueOptions(queueName);
        
        return new CreateQueueOptions(queueName)
        {
            LockDuration = queueMeta.LockDuration ?? defaults.LockDuration,
            RequiresSession = queueMeta.RequiresSession ?? defaults.RequiresSession,
            DeadLetteringOnMessageExpiration = queueMeta.DeadLetteringOnMessageExpiration ?? defaults.DeadLetteringOnMessageExpiration,
            MaxDeliveryCount = queueMeta.MaxDeliveryCount ?? defaults.MaxDeliveryCount,
            ForwardDeadLetteredMessagesTo = queueMeta.ForwardDeadLetteredMessagesTo,
            MaxSizeInMegabytes = queueMeta.MaxSizeInMegabytes ?? defaults.MaxSizeInMegabytes,
            RequiresDuplicateDetection = queueMeta.RequiresDuplicateDetection ?? defaults.RequiresDuplicateDetection,
            DefaultMessageTimeToLive = queueMeta.DefaultMessageTimeToLive ?? defaults.DefaultMessageTimeToLive,
            DuplicateDetectionHistoryTimeWindow = queueMeta.DuplicateDetectionHistoryTimeWindow ?? defaults.DuplicateDetectionHistoryTimeWindow,
            EnableBatchedOperations = queueMeta.EnableBatchedOperations ?? defaults.EnableBatchedOperations,
            EnablePartitioning = queueMeta.EnablePartitioning ?? defaults.EnablePartitioning
        };
    }
}