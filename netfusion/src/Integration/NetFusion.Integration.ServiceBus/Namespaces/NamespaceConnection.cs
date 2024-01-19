using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.ServiceBus.Plugin.Settings;
using NetFusion.Integration.ServiceBus.Rpc.Metadata;

namespace NetFusion.Integration.ServiceBus.Namespaces;

/// <summary>
/// Class instance created for each configured Azure Service Bus Namespace.
/// Contains clients for sending/subscribing and creating namespace entities.
/// </summary>
public class NamespaceConnection
{
    private readonly ILogger<NamespaceConnection> _logger;
    
    /// <summary>
    /// Associated connection settings.
    /// </summary>
    private NamespaceSettings NamespaceSettings { get; }
    
    /// <summary>
    /// Client used to publish and subscribe to namespace entities.
    /// </summary>
    public ServiceBusClient BusClient { get; }
    
    /// <summary>
    /// Client used to manage namespace entities.
    /// </summary>
    public ServiceBusAdministrationClient AdminClient { get; }

    private readonly ExternalEntitySettings _externalSettings;

    internal NamespaceConnection(ILogger<NamespaceConnection> logger, NamespaceSettings namespaceSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        NamespaceSettings = namespaceSettings ?? throw new ArgumentNullException(nameof(namespaceSettings));
        
        BusClient = new ServiceBusClient(NamespaceSettings.Host, new DefaultAzureCredential(), BuildOptions());
        AdminClient = new ServiceBusAdministrationClient(NamespaceSettings.Host, new DefaultAzureCredential());
        
        _externalSettings = new ExternalEntitySettings(namespaceSettings);
    }
    
    // --------------------------------------------------------------------------
    
    private ServiceBusClientOptions BuildOptions()
    {
        var clientOptions = new ServiceBusClientOptions
        {
            Identifier = NamespaceSettings.HostPluginId
        };
        
        clientOptions.TransportType = NamespaceSettings.TransportType ?? clientOptions.TransportType;

        var retrySettings = NamespaceSettings.RetrySettings;
        if (retrySettings != null)
        {
            var clientRetryOptions = clientOptions.RetryOptions;
            
            clientRetryOptions.Mode = retrySettings.Mode ?? clientRetryOptions.Mode;
            clientRetryOptions.Delay = retrySettings.Delay ?? clientRetryOptions.Delay;
            clientRetryOptions.MaxDelay = retrySettings.MaxDelay ?? clientRetryOptions.MaxDelay;
            clientRetryOptions.MaxRetries = retrySettings.MaxRetries ?? clientRetryOptions.MaxRetries;
            clientRetryOptions.TryTimeout = retrySettings.TryTimeout ?? clientRetryOptions.TryTimeout;
        }

        _logger.Log(
            LogMessage.For(LogLevel.Debug, "Client options for {Namespace}", NamespaceSettings.Name)
                .WithProperties(LogProperty.ForName("Client-Options", clientOptions)));
        
        return clientOptions;
    }

    /// <summary>
    /// Disposes clients with persistent connections.
    /// </summary>
    /// <returns>Future Task Result</returns>
    public async Task CloseClientsAsync()
    {
        await BusClient.DisposeAsync().AsTask();
    }

    public async Task CreateOrUpdateQueue(string queueName, CreateQueueOptions queue)
    {
        if (queue == null) throw new ArgumentNullException(nameof(queue));
        
        _externalSettings.ApplyQueueSettings(queueName, queue);
        
        if (! await UpdateExistingQueue(queueName, queue))
        {
            try
            {
                LogCreatingEntity(queueName, queue);
               await AdminClient.CreateQueueAsync(queue);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                await UpdateExistingQueue(queueName, queue);
            }
        }
    }
    
    private async Task<bool> UpdateExistingQueue(string queueName, CreateQueueOptions queue)
    {
        if (!await AdminClient.QueueExistsAsync(queue.Name)) return false;
        
        LogUpdatingEntity(queueName, queue);
        
        QueueProperties queueProperties = await AdminClient.GetQueueAsync(queue.Name);
        queueProperties.Update(queue);
        await AdminClient.UpdateQueueAsync(queueProperties);
        return true;

    }

    public async Task CreateOrUpdateTopic(string topicName, CreateTopicOptions topic)
    {
        _externalSettings.ApplyTopicSettings(topicName, topic);
        
        if (! await UpdateExistingTopic(topicName, topic))
        {
            try
            {
                LogCreatingEntity(topicName, topic);
                await AdminClient.CreateTopicAsync(topic);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                await UpdateExistingTopic(topicName, topic);
            }
        }
    }

    private async Task<bool> UpdateExistingTopic(string topicName, CreateTopicOptions topic)
    {
        if (!await AdminClient.TopicExistsAsync(topic.Name)) return false;
        
        LogUpdatingEntity(topicName, topic);
        
        TopicProperties topicProperties = await AdminClient.GetTopicAsync(topic.Name);
        topicProperties.Update(topic);
        await AdminClient.UpdateTopicAsync(topicProperties);
        return true;

    }
    
    public async Task CreateOrUpdateSubscription(
        string subscriptionName,
        CreateSubscriptionOptions subscription,
        CreateRuleOptions[] roles)
    {
        await CreateTempTopic(subscription);
        
        _externalSettings.ApplySubscriptionSettings(subscriptionName, subscription);
        roles = _externalSettings.ApplySubscriptionRoleSettings(subscriptionName, roles);
        
        if (! await UpdateExistingSubscription(subscriptionName, subscription, roles))
        {
            try
            {
                LogUpdatingEntity(subscriptionName, subscription);
                await AdminClient.CreateSubscriptionAsync(subscription);
                await UpdateRules(subscriptionName, subscription, roles);
            }
            catch (ServiceBusException ex) 
                when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                await UpdateExistingSubscription(subscriptionName, subscription, roles);
            }
        }
    }

    public void ApplyExternalQueueProcessingSettings(string queueName, ServiceBusProcessorOptions processor)
    {
        if (!NamespaceSettings.Queues.TryGetValue(queueName, out var settings)) return;

        processor.Identifier = NamespaceSettings.HostPluginId;
        processor.PrefetchCount = settings.PrefetchCount ?? processor.PrefetchCount;
        processor.MaxConcurrentCalls = settings.MaxConcurrentCalls ?? processor.MaxConcurrentCalls;
    }
    
    public void ApplyExternalRpcQueueProcessingSettings(string queueName, RpcProcessingOptions processor)
    {
        if (!NamespaceSettings.Queues.TryGetValue(queueName, out var settings)) return;
        
        processor.Identifier = NamespaceSettings.HostPluginId;
        processor.PrefetchCount = settings.PrefetchCount ?? processor.PrefetchCount;
        processor.MaxConcurrentCalls = settings.MaxConcurrentCalls ?? processor.MaxConcurrentCalls;
    }

    public void ApplyExternalSubscriptionProcessorSettings(string subscriptionName, ServiceBusProcessorOptions processor)
    {
        if (! NamespaceSettings.Subscriptions.TryGetValue(subscriptionName, out var settings)) return;
        
        processor.Identifier = NamespaceSettings.HostPluginId;
        processor.PrefetchCount = settings.PrefetchCount ?? processor.PrefetchCount;
        processor.MaxConcurrentCalls = settings.MaxConcurrentCalls ?? processor.MaxConcurrentCalls;
    }
    
    // In a Microservice architecture, it is not guaranteed that the publishing service,
    // responsible for creating topics, will be started before consumers of the topics.
    // This creates a temporary topic that is subscribed and reconfigured when the
    // publisher starts.
    private async Task CreateTempTopic(CreateSubscriptionOptions subscription)
    {
        if (! await AdminClient.TopicExistsAsync(subscription.TopicName))
        {
            try
            {
                await AdminClient.CreateTopicAsync(subscription.TopicName);
            }
            catch (ServiceBusException ex) 
                when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                // Ignore exception if the topic was created between checking
                // if existed and issuing creation request.
            }
        }
    }

    private async Task<bool> UpdateExistingSubscription(
        string subscriptionName,
        CreateSubscriptionOptions subscription,
        CreateRuleOptions[] roles)
    {
        if (!await AdminClient.SubscriptionExistsAsync(subscription.TopicName, subscription.SubscriptionName))
        {
            return false;
        }
        
        LogUpdatingEntity(subscriptionName, subscription);

        SubscriptionProperties subscriptionProps = await AdminClient.GetSubscriptionAsync(
            subscription.TopicName, subscription.SubscriptionName);

        subscriptionProps.Update(subscription);
            
        await AdminClient.UpdateSubscriptionAsync(subscriptionProps);
        await UpdateRules(subscriptionName, subscription, roles);
        return true;

    }
    
    private async Task UpdateRules(string subscriptionName, CreateSubscriptionOptions subscription,
        CreateRuleOptions[] roles)
    {
        if (!roles.Any()) return;
        
        LogUpdatingEntity($"{subscriptionName}-roles", roles);
        
        await SyncRules(subscription, roles);
        await UpdateDefaultRule(subscription, roles);
    }
    
    private async Task SyncRules(CreateSubscriptionOptions subscription, CreateRuleOptions[] roles)
    {
        var existingRules = await ListRules(subscription).ToArrayAsync();

        var rolesToDelete = existingRules.Where(r => roles.All(ro => ro.Name != r.Name));
        foreach (var rule in rolesToDelete) await DeleteRule(subscription, rule.Name);
        
        var rolesToAdd = roles.Where(ro => existingRules.All(r => r.Name != ro.Name));
        foreach (var rule in rolesToAdd) await CreateRule(subscription, rule);
        
        var rolesToUpdate = roles.Where(ro => existingRules.Count(r => r.Name == ro.Name) == 1);
        foreach (var rule in rolesToUpdate) await UpdateRule(subscription, existingRules, rule);
    }
    
    private async Task DeleteRule(CreateSubscriptionOptions subscription, string ruleName)
    {
        if (await AdminClient.RuleExistsAsync(subscription.TopicName, subscription.SubscriptionName, ruleName))
        {
            try { await AdminClient.DeleteRuleAsync(subscription.TopicName, subscription.SubscriptionName, ruleName); }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) { }
        }
    }

    private async Task CreateRule(CreateSubscriptionOptions subscription, CreateRuleOptions rule)
    {
        if (! await AdminClient.RuleExistsAsync(subscription.TopicName, subscription.SubscriptionName, rule.Name))
        {
            try { await AdminClient.CreateRuleAsync(subscription.TopicName, subscription.SubscriptionName, rule); }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists) { }
        }
    }

    private async Task UpdateRule(CreateSubscriptionOptions subscription, IEnumerable<RuleProperties> rules, CreateRuleOptions rule)
    {
        var existingRule = rules.First(er => er.Name == rule.Name);
        existingRule.Filter = rule.Filter;
            
        await AdminClient.UpdateRuleAsync(subscription.TopicName, subscription.SubscriptionName, existingRule);
    }
    
    // Based on the list of rules associated with a subscription, determines if the $Default rule applies.
    // When custom rules are applied to a topic subscription, the $Default rule must be removed so only
    // the custom rules apply.  If no custom roles are specified, the $Default rule must either remain or
    // be added back.  The $Default rule is coded as (1=1) so all messages will apply.
    private async Task UpdateDefaultRule(CreateSubscriptionOptions subscription, IEnumerable<CreateRuleOptions> roles)
    {
        if (roles.Any())
        {
            await DeleteRule(subscription, CreateRuleOptions.DefaultRuleName);
        }
        else
        {
            await CreateRule(subscription, new CreateRuleOptions(CreateRuleOptions.DefaultRuleName));
        }
    }
    
    // Returns all the roles associated with a topic subscription.  If there are that many rules
    // that paging applies, then we surely have an issue!
    private async IAsyncEnumerable<RuleProperties> ListRules(CreateSubscriptionOptions subscription)
    {
        string? token = null;
        do
        {
            await foreach (var page in AdminClient
                .GetRulesAsync(subscription.TopicName, subscription.SubscriptionName)
                .AsPages(token))
            {
                token = page.ContinuationToken;
                foreach (var rule in page.Values)
                {
                    if (rule.Name != CreateRuleOptions.DefaultRuleName) yield return rule;
                }
                
            }
        } while (token != null);
    }
    
    private void LogCreatingEntity(string entityName, object options) =>
        _logger.Log(
            LogMessage.For(LogLevel.Debug, "Creating Entity {EntityName}", entityName)
                .WithProperties(LogProperty.ForName("Options", options))
        );
    
    private void LogUpdatingEntity(string entityName, object options) =>
        _logger.Log(
            LogMessage.For(LogLevel.Debug, "Updating Entity {EntityName}", entityName)
                .WithProperties(LogProperty.ForName("Options", options))
        );
}

