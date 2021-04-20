using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher;
using NetFusion.Azure.ServiceBus.Settings;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Base.Logging;

namespace NetFusion.Azure.ServiceBus.Namespaces
{
    /// <summary>
    /// Class instance created for each configured Azure Service Bus Namespace.
    /// Contains clients for sending/subscribing and creating Namespace entities.
    /// </summary>
    public class NamespaceConnection
    {
        private readonly IExtendedLogger _logger;
        
        // Dependent Module Services:
        public NamespaceSettings BusNamespace { get; }
        
        // Azure Service Bus clients:
        public ServiceBusClient BusClient { get; private set; }
        public ServiceBusAdministrationClient AdminClient { get; private set; }

        internal NamespaceConnection(IExtendedLogger logger, NamespaceSettings busNamespace)
        {
            _logger = logger;
            BusNamespace = busNamespace ?? throw new ArgumentNullException(nameof(busNamespace));
        }
        
        // -------------------------------- Logging ---------------------------------

        private void LogEntity(string action, string entityType, NamespaceEntity entity)
        {
            _logger.Log<NamespaceConnection>(LogLevel.Debug, $"{action} {entityType}: {entity}");
        }
        
        private void LogSubscription(string action, string entityType, EntitySubscription subscription)
        {
            _logger.Log<NamespaceConnection>(LogLevel.Debug, $"{action} {entityType}: {subscription}");
        }
        
        private void LogRule(string action, TopicSubscription subscription, string ruleName)
        {
            _logger.Log<NamespaceConnection>(LogLevel.Debug, $"{action} Rule: {ruleName} on {subscription}");
        }
        
        // --------------------------------------------------------------------------

        /// <summary>
        /// Instantiates a client for creating namespace entities and a client for sending
        /// and receiving messages from the created entities. 
        /// </summary>
        public void CreateClients()
        {
            BusClient = new ServiceBusClient(BusNamespace.ConnString, BuildOptions());
            AdminClient = new ServiceBusAdministrationClient(BusNamespace.ConnString);
        }

        private ServiceBusClientOptions BuildOptions()
        {
            var defaultOptions = new ServiceBusClientOptions();
            defaultOptions.TransportType = BusNamespace.TransportType ?? defaultOptions.TransportType;

            var retrySettings = BusNamespace.RetrySettings;
            if (retrySettings != null)
            {
                var defaultRetryOptions = defaultOptions.RetryOptions;
                
                defaultRetryOptions.Mode = retrySettings.Mode ?? defaultRetryOptions.Mode;
                defaultRetryOptions.Delay = retrySettings.Delay ?? defaultRetryOptions.Delay;
                defaultRetryOptions.MaxDelay = retrySettings.MaxDelay ?? defaultRetryOptions.MaxDelay;
                defaultRetryOptions.MaxRetries = retrySettings.MaxRetries ?? defaultRetryOptions.MaxRetries;
                defaultRetryOptions.TryTimeout = retrySettings.TryTimeout ?? defaultRetryOptions.TryTimeout;
            }

            return defaultOptions;
        }

        /// <summary>
        /// Disposes clients with persistent connections.
        /// </summary>
        /// <returns>Future Task Result</returns>
        public async Task CloseClientsAsync()
        {
            if (BusClient != null)
            {
                await BusClient.DisposeAsync().AsTask();
            }
        }
        
        // ---------------------- Topics --------------------------------------

        /// <summary>
        /// Determines if the topic has already been created.  If found, the properties
        /// of the existing topic are updated.  Otherwise, the topic is created.
        /// </summary>
        /// <param name="topicMeta">Metadata describing the topic.</param>
        /// <returns>Future Result Task</returns>
        public async Task CreateOrUpdateTopic(TopicMeta topicMeta)
        {
            if (! await UpdateExistingTopic(topicMeta))
            {
                try
                {
                    LogEntity("Creating", "Topic", topicMeta);
                    await AdminClient.CreateTopicAsync(topicMeta.ToCreateOptions());
                }
                catch (ServiceBusException ex) 
                    when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    await UpdateExistingTopic(topicMeta);
                }
            }
        }
        
        private async Task<bool> UpdateExistingTopic(TopicMeta topicMeta)
        {
            if (await AdminClient.TopicExistsAsync(topicMeta.EntityName))
            {
                LogEntity("Updating", "Topic", topicMeta);
                
                TopicProperties topicProps = await AdminClient.GetTopicAsync(topicMeta.EntityName);

                topicMeta.UpdateProperties(topicProps);
                await AdminClient.UpdateTopicAsync(topicProps);
                return true;
            }

            return false;
        }
        
        // ---------------------- Queues ---------------------------------
        
        /// <summary>
        /// Determines if the queue has already been created.  If found, the properties
        /// of the existing queue are updated.  Otherwise, the queue is created.
        /// </summary>
        /// <param name="queueMeta">Metadata describing the queue.</param>
        /// <returns>Future Result Task</returns>
        public async Task CreateOrUpdateQueue(QueueMeta queueMeta)
        {
            if (! await UpdateExistingQueue(queueMeta))
            {
                try
                {
                    LogEntity("Creating", "Queue", queueMeta);
                    await AdminClient.CreateQueueAsync(queueMeta.ToCreateOptions());
                }
                catch (ServiceBusException ex) 
                    when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    await UpdateExistingQueue(queueMeta);
                }
            }
        }
        
        private async Task<bool> UpdateExistingQueue(QueueMeta queueMeta)
        {
            if (await AdminClient.QueueExistsAsync(queueMeta.EntityName))
            {
                LogEntity("Updating", "Queue", queueMeta);
                
                QueueProperties queueProps = await AdminClient.GetQueueAsync(queueMeta.EntityName);

                queueMeta.UpdateProperties(queueProps);
                await AdminClient.UpdateQueueAsync(queueProps);
                return true;
            }

            return false;
        }
        
        // ---------------------- Subscriptions --------------------------

        /// <summary>
        /// Determines if the subscription has already been created.  If found, the properties
        /// of the existing subscription and rules are updated.  Otherwise, the subscription
        /// with any configured rules is created.
        /// </summary>
        /// <param name="subscription">The subscription to be created or updated.</param>
        /// <returns>Future Result Task</returns>
        public async Task CreateOrUpdateSubscription(TopicSubscription subscription)
        {
            if (! await UpdateExistingSubscription(subscription))
            {
                try
                {
                    LogSubscription("Creating", "Subscription", subscription);
                    
                    await AdminClient.CreateSubscriptionAsync(subscription.ToCreateOptions());
                    await UpdateRules(subscription);
                }
                catch (ServiceBusException ex) 
                    when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    await UpdateExistingSubscription(subscription);
                }
            }
        }

        private async Task<bool> UpdateExistingSubscription(TopicSubscription subscription)
        {
            if (await AdminClient.SubscriptionExistsAsync(subscription.EntityName, subscription.UniqueSubscriptionName))
            {
                LogSubscription("Updating", "Subscription", subscription);
                
                SubscriptionProperties subscriptionProps = await AdminClient.GetSubscriptionAsync(
                    subscription.EntityName, subscription.UniqueSubscriptionName);

                subscription.UpdateSubscriptionProperties(subscriptionProps);
                
                await AdminClient.UpdateSubscriptionAsync(subscriptionProps);
                await UpdateRules(subscription);
                return true;
            }

            return false;
        }
        
        private async Task UpdateRules(TopicSubscription subscription)
        {
            await SyncRules(subscription);
            await UpdateDefaultRule(subscription);
        }
        
        private async Task SyncRules(TopicSubscription subscription)
        {
            var existingRules = await ListRules(subscription).ToArrayAsync();

            var rolesToDelete = existingRules.Where(r => subscription.RuleOptions.Count(ro => ro.Name == r.Name) == 0);
            foreach (var rule in rolesToDelete) await DeleteRule(subscription, rule.Name);
            
            var rolesToAdd = subscription.RuleOptions.Where(ro => existingRules.Count(r => r.Name == ro.Name) == 0);
            foreach (var rule in rolesToAdd) await CreateRule(subscription, rule);
            
            var rolesToUpdate = subscription.RuleOptions.Where(ro => existingRules.Count(r => r.Name == ro.Name) == 1);
            foreach (var rule in rolesToUpdate) await UpdateRule(subscription, existingRules, rule);
        }
        
        private async Task DeleteRule(TopicSubscription subscription, string ruleName)
        {
            if (await AdminClient.RuleExistsAsync(subscription.EntityName, subscription.UniqueSubscriptionName, ruleName))
            {
                LogRule("Deleting", subscription, ruleName);
                
                try { await AdminClient.DeleteRuleAsync(subscription.EntityName, subscription.UniqueSubscriptionName, ruleName); }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) { }
            }
        }

        private async Task CreateRule(TopicSubscription subscription, CreateRuleOptions rule)
        {
            if (! await AdminClient.RuleExistsAsync(subscription.EntityName, subscription.UniqueSubscriptionName, rule.Name))
            {
                LogRule("Creating", subscription, rule.Name);
                
                try { await AdminClient.CreateRuleAsync(subscription.EntityName, subscription.UniqueSubscriptionName, rule); }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists) { }
            }
        }

        private async Task UpdateRule(TopicSubscription subscription, IEnumerable<RuleProperties> rules, CreateRuleOptions rule)
        {
            LogRule("Updating", subscription, rule.Name);
            
            var existingRule = rules.First(er => er.Name == rule.Name);
            existingRule.Filter = rule.Filter;
                
            await AdminClient.UpdateRuleAsync(subscription.EntityName, subscription.UniqueSubscriptionName, existingRule);
        }
        
        // Based on the list of rules associated with a subscription, determines if the $Default rule applies.
        // When custom rules are applied to a topic subscription, the $Default rule must be removed so only
        // the custom rules apply.  If no custom roles are specified, the $Default rule must either remain or
        // be added back.  The $Default rule is coded as (1=1) so all messages will apply.
        private async Task UpdateDefaultRule(TopicSubscription subscription)
        {
            if (subscription.RuleOptions.Any())
            {
                LogRule("Deleting", subscription, CreateRuleOptions.DefaultRuleName);
                await DeleteRule(subscription, CreateRuleOptions.DefaultRuleName);
            }
            else
            {
                LogRule("Creating", subscription, CreateRuleOptions.DefaultRuleName);
                await CreateRule(subscription, new CreateRuleOptions(CreateRuleOptions.DefaultRuleName));
            }
        }
        
        // Returns all the roles associated with a topic subscription.  If there are that many rules
        // that paging applies, then we surely have an issue!
        private async IAsyncEnumerable<RuleProperties> ListRules(TopicSubscription subscription)
        {
            string token = null;
            do
            {
                await foreach (var page in AdminClient
                    .GetRulesAsync(subscription.EntityName, subscription.UniqueSubscriptionName)
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
    }
}