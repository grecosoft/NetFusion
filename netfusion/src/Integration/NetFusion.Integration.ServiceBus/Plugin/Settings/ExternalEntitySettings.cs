using Azure.Messaging.ServiceBus.Administration;

namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

/// <summary>
/// Responsible for applying namespace entity settings stored externally to the corresponding
/// namespace entity defined within code.
/// </summary>
public class ExternalEntitySettings(NamespaceSettings namespaceSettings)
{
    private readonly NamespaceSettings _namespaceSettings = namespaceSettings ?? 
        throw new ArgumentNullException(nameof(namespaceSettings));

    public void ApplyQueueSettings(string queueName, CreateQueueOptions queueOptions)
    {
        ArgumentNullException.ThrowIfNull(queueOptions);
        if (! _namespaceSettings.Queues.TryGetValue(queueName, out QueueSettings? queueSettings)) return;
        
        if (queueSettings.LockDurationInSeconds != null)
        {
            queueOptions.LockDuration = TimeSpan.FromSeconds(queueSettings.LockDurationInSeconds.Value);
        }

        if (queueSettings.MaxDeliveryCount != null)
        {
            queueOptions.MaxDeliveryCount = queueSettings.MaxDeliveryCount.Value;
        }

        if (queueSettings.MaxSizeInMegabytes != null)
        {
            queueOptions.MaxSizeInMegabytes = queueSettings.MaxSizeInMegabytes.Value;
        }

        if (queueSettings.ForwardDeadLetteredMessagesTo != null)
        {
            queueOptions.ForwardDeadLetteredMessagesTo = queueSettings.ForwardDeadLetteredMessagesTo;
        }

        if (queueSettings.DeadLetteringOnMessageExpiration != null)
        {
            queueOptions.DeadLetteringOnMessageExpiration = queueSettings.DeadLetteringOnMessageExpiration.Value;
        }
    }
    
    public void ApplyTopicSettings(string topicName, CreateTopicOptions topicOptions)
    {
        ArgumentNullException.ThrowIfNull(topicOptions);
        if (!_namespaceSettings.Topics.TryGetValue(topicName, out TopicSettings? topicSettings)) return;

        if (topicSettings.MaxSizeInMegabytes != null)
        {
            topicOptions.MaxSizeInMegabytes = topicSettings.MaxSizeInMegabytes.Value;
        }

        if (topicSettings.MaxSizeInMegabytes != null)
        {
            topicOptions.MaxMessageSizeInKilobytes = topicSettings.MaxMessageSizeInKilobytes;
        }

        if (topicSettings.DefaultMessageTimeToLiveInSeconds != null)
        {
            topicOptions.DefaultMessageTimeToLive = TimeSpan.FromSeconds(topicSettings.DefaultMessageTimeToLiveInSeconds.Value);
        }
    }
    
    public void ApplySubscriptionSettings(string subscriptionName, CreateSubscriptionOptions subscriptionOptions)
    {
        ArgumentNullException.ThrowIfNull(subscriptionOptions);
        if (!_namespaceSettings.Subscriptions.TryGetValue(subscriptionName, out var settings)) return;

        if (settings.LockDurationInSeconds != null)
        {
            subscriptionOptions.LockDuration = TimeSpan.FromSeconds(settings.LockDurationInSeconds.Value);
        }

        if (settings.DefaultMessageTimeToLiveInSeconds != null)
        {
            subscriptionOptions.DefaultMessageTimeToLive = 
                TimeSpan.FromSeconds(settings.DefaultMessageTimeToLiveInSeconds.Value);
        }

        if (settings.MaxDeliveryCount != null)
        {
            subscriptionOptions.MaxDeliveryCount = settings.MaxDeliveryCount.Value;
        }

        if (settings.DeadLetteringOnMessageExpiration != null)
        {
            subscriptionOptions.DeadLetteringOnMessageExpiration = settings.DeadLetteringOnMessageExpiration.Value;
        }

        if (settings.ForwardDeadLetteredMessagesTo != null)
        {
            subscriptionOptions.ForwardDeadLetteredMessagesTo = settings.ForwardDeadLetteredMessagesTo;
        }

        if (settings.EnableDeadLetteringOnFilterEvaluationExceptions != null)
        {
            subscriptionOptions.EnableDeadLetteringOnFilterEvaluationExceptions =
                settings.EnableDeadLetteringOnFilterEvaluationExceptions.Value;
        }
    }
    
    public CreateRuleOptions[] ApplySubscriptionRoleSettings(string subscriptionName, 
        IEnumerable<CreateRuleOptions> roleOptions)
    {
        if (string.IsNullOrWhiteSpace(subscriptionName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(subscriptionName));
        
        var rolesToUpdate = roleOptions.ToList();
        
        if (! _namespaceSettings.Subscriptions.TryGetValue(subscriptionName, out var settings))
        {
            return rolesToUpdate.ToArray();
        }
        
        foreach (var (name, role) in settings.Rules)
        {
            CreateRuleOptions? existingRole = rolesToUpdate.FirstOrDefault(r => r.Name == name);
            if (existingRole == null)
            {
                existingRole = new CreateRuleOptions(name);
                rolesToUpdate.Add(existingRole);
            }

            existingRole.Filter = new SqlRuleFilter(role.Filter);

            if (role.Action != null)
            {
                existingRole.Action = new SqlRuleAction(role.Action);
            }
        }

        return rolesToUpdate.ToArray();
    }
}