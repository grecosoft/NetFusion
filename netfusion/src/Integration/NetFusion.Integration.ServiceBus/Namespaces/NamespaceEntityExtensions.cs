using Azure.Messaging.ServiceBus.Administration;

namespace NetFusion.Integration.ServiceBus.Namespaces;

/// <summary>
/// Extension methods used to update an existing entity properties
/// from its corresponding creation options.
/// </summary>
public static class NamespaceEntityExtensions
{
    public static void Update(this QueueProperties queueProperties,
        CreateQueueOptions queueOptions)
    {
        queueProperties.MaxSizeInMegabytes = queueOptions.MaxSizeInMegabytes;
        queueProperties.ForwardTo = queueOptions.ForwardTo;
        queueProperties.LockDuration = queueOptions.LockDuration;
        queueProperties.EnableBatchedOperations = queueOptions.EnableBatchedOperations;
        queueProperties.MaxDeliveryCount = queueOptions.MaxDeliveryCount;
        queueProperties.AutoDeleteOnIdle = queueOptions.AutoDeleteOnIdle;
        queueProperties.MaxSizeInMegabytes = queueOptions.MaxSizeInMegabytes;
        queueProperties.DeadLetteringOnMessageExpiration = queueOptions.DeadLetteringOnMessageExpiration;
        queueProperties.DefaultMessageTimeToLive = queueOptions.DefaultMessageTimeToLive;
        queueProperties.DuplicateDetectionHistoryTimeWindow = queueOptions.DuplicateDetectionHistoryTimeWindow;
        queueProperties.ForwardDeadLetteredMessagesTo = queueOptions.ForwardDeadLetteredMessagesTo;
    }

    public static void Update(this TopicProperties topicProperties,
        CreateTopicOptions topicOptions)
    {
        topicProperties.EnableBatchedOperations = topicOptions.EnableBatchedOperations;
        topicProperties.AutoDeleteOnIdle = topicOptions.AutoDeleteOnIdle;
        topicProperties.DefaultMessageTimeToLive = topicOptions.DefaultMessageTimeToLive;
        topicProperties.DuplicateDetectionHistoryTimeWindow = topicOptions.DuplicateDetectionHistoryTimeWindow;
        topicProperties.MaxSizeInMegabytes = topicOptions.MaxSizeInMegabytes;
        topicProperties.EnablePartitioning = topicOptions.EnablePartitioning;
        topicProperties.SupportOrdering = topicOptions.SupportOrdering;
        topicProperties.RequiresDuplicateDetection = topicOptions.RequiresDuplicateDetection;
    }

    public static void Update(this SubscriptionProperties subscriptionProperties,
        CreateSubscriptionOptions subscriptionOptions)
    {
        subscriptionProperties.ForwardTo = subscriptionOptions.ForwardTo;
        subscriptionProperties.LockDuration = subscriptionOptions.LockDuration;
        subscriptionProperties.EnableBatchedOperations = subscriptionOptions.EnableBatchedOperations;
        subscriptionProperties.AutoDeleteOnIdle = subscriptionOptions.AutoDeleteOnIdle;
        subscriptionProperties.RequiresSession = subscriptionOptions.RequiresSession;
        subscriptionProperties.MaxDeliveryCount = subscriptionOptions.MaxDeliveryCount;
        subscriptionProperties.DeadLetteringOnMessageExpiration = subscriptionOptions.DeadLetteringOnMessageExpiration;
        subscriptionProperties.DefaultMessageTimeToLive = subscriptionOptions.DefaultMessageTimeToLive;
        subscriptionProperties.ForwardDeadLetteredMessagesTo = subscriptionOptions.ForwardDeadLetteredMessagesTo;
        
        subscriptionProperties.EnableDeadLetteringOnFilterEvaluationExceptions = 
            subscriptionOptions.EnableDeadLetteringOnFilterEvaluationExceptions;
    }
}