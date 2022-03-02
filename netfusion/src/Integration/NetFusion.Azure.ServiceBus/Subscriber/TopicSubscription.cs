using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Azure.ServiceBus.Settings;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Strategies;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Contains information specific to a Topic Subscription.  By default, a topic subscription's name
    /// has the identity value of the running Microservice host appended.  This allows the same subscription
    /// names to be used between multiple different consuming Microservices.  More importantly, if multiple
    /// instances of a consuming Microservice are consuming the same subscription, messages will be delivered
    /// round-robin among the consumers.  On the other hand, if a unique quid is appended to the subscription
    /// name, a unique subscription per consuming Microservice is created.  This is used when all Microservice
    /// running consumers should receive the message.  This is used in cases for non persistent notifications.
    /// </summary>
    public class TopicSubscription : EntitySubscription
    {
        /// <summary>
        /// The name used to identity the subscription.
        /// </summary>
        public string SubscriptionName { get; }
        
        /// <summary>
        /// Indicates that messages should be delivered to all consumers and not round-robin.  If set, an
        /// unique quid is append to the subscription name making it unique per running Microservice instance.
        /// </summary>
        public bool IsFanout { get; }

        private readonly IDictionary<string, CreateRuleOptions> _rules = new Dictionary<string, CreateRuleOptions>();

        public TopicSubscription(string namespaceName, string topicName, string subscriptionName, bool isFanout)
            : base(namespaceName, topicName)
        {
            if (string.IsNullOrWhiteSpace(subscriptionName))
                throw new ArgumentException("Subscription Name not specified.", nameof(subscriptionName));

            SubscriptionName = subscriptionName;
            SettingsKey = $"{topicName}|{subscriptionName}";
            IsFanout = isFanout;
            SubscriptionStrategy = new TopicSubscriptionStrategy(this);
            
            // TODO: Should HostId be appended?
            UniqueSubscriptionName = IsFanout ? $"{SubscriptionName}_{Guid.NewGuid()}" : SubscriptionName;
        }

        internal override void ApplySettings(SubscriptionSettings settings)
        {
            base.ApplySettings(settings);
            foreach (var (name, rule) in settings.Rules)
            {
                AddRule(name, new SqlRuleFilter(rule.Filter));
            }
        }

        // ----------------------- Configuration Overrides ----------------------

        /// <summary>
        /// Duration of a peek lock receive. i.e., the amount of time that the message is locked by a given receiver so that
        /// no other receiver receives the same message.
        /// </summary>
        /// <remarks>Max value is 5 minutes. Default value is 60 seconds.</remarks>
        public TimeSpan? LockDuration { get; set; } 

        /// <summary>
        /// This indicates whether the subscription supports the concept of session. Sessionful-messages follow FIFO ordering.
        /// </summary>
        public bool? RequiresSession { get; set; }

        /// <summary>
        /// The default time to live value for the messages. This is the duration after which the message expires, starting from when
        /// the message is sent to Service Bus.
        /// </summary>
        public TimeSpan? DefaultMessageTimeToLive { get; set; } 

        /// <summary>
        /// The <see cref="TimeSpan"/> idle interval after which the subscription is automatically deleted.
        /// </summary>
        public TimeSpan? AutoDeleteOnIdle { get; set; }

        /// <summary>
        /// Indicates whether this subscription has dead letter support when a message expires.
        /// </summary>
        public bool? DeadLetteringOnMessageExpiration { get; set; }

        /// <summary>
        /// indicates whether messages need to be forwarded to dead-letter sub queue when subscription rule evaluation fails.
        /// </summary>
        /// <remarks>Defaults to true.</remarks>
        public bool? EnableDeadLetteringOnFilterEvaluationExceptions { get; set; } 

        /// <summary>
        /// The maximum delivery count of a message before it is dead-lettered.
        /// </summary>
        public int? MaxDeliveryCount { get; set; } 

        /// <summary>
        /// Indicates whether server-side batched operations are enabled.
        /// </summary>
        public bool? EnableBatchedOperations { get; set; } 

        /// <summary>
        /// The name of the recipient entity to which all the messages sent to the subscription are forwarded to.
        /// </summary>
        public string ForwardTo { get; set; }

        /// <summary>
        /// The name of the recipient entity to which all the dead-lettered messages of this subscription are forwarded to.
        /// </summary>
        public string ForwardDeadLetteredMessagesTo { get; set; }

        // ----------------------- Internal Interface ----------------------

        /// <summary>
        /// Unique name consisting of the base SubscriptionName with either the Microservice's host Id appended
        /// or an unique GUID value.  The type of appended Id value determines if the massages sent to the subscription
        /// are either processed by competing consumer Microservices or dispatched to all consumers as notifications. 
        /// </summary>
        internal string UniqueSubscriptionName { get; private set; }

        /// <summary>
        /// List of roles associated with the subscription.
        /// </summary>
        internal IEnumerable<CreateRuleOptions> RuleOptions => _rules.Values;
        
        internal CreateSubscriptionOptions ToCreateOptions()
        {
            return new(EntityName, UniqueSubscriptionName)
            {
                LockDuration = LockDuration ?? TimeSpan.FromSeconds(60),
                RequiresSession = RequiresSession ?? false,
                DefaultMessageTimeToLive = DefaultMessageTimeToLive ?? TimeSpan.MaxValue,
                AutoDeleteOnIdle = AutoDeleteOnIdle ?? TimeSpan.MaxValue,
                DeadLetteringOnMessageExpiration = DeadLetteringOnMessageExpiration ?? false,
                EnableDeadLetteringOnFilterEvaluationExceptions = EnableDeadLetteringOnFilterEvaluationExceptions ?? true,
                MaxDeliveryCount = MaxDeliveryCount ?? 10,
                EnableBatchedOperations = EnableBatchedOperations ?? true,
                ForwardTo = ForwardTo,
                ForwardDeadLetteredMessagesTo = ForwardDeadLetteredMessagesTo
            };
        }

        internal void UpdateSubscriptionProperties(SubscriptionProperties properties)
        {
            properties.LockDuration = LockDuration ?? TimeSpan.FromSeconds(60);
            properties.RequiresSession = RequiresSession ?? false;
            properties.DefaultMessageTimeToLive = DefaultMessageTimeToLive ?? TimeSpan.MaxValue;
            properties.AutoDeleteOnIdle = AutoDeleteOnIdle ?? TimeSpan.MaxValue;
            properties.DeadLetteringOnMessageExpiration = DeadLetteringOnMessageExpiration ?? false;
            properties.EnableDeadLetteringOnFilterEvaluationExceptions = EnableDeadLetteringOnFilterEvaluationExceptions ?? true;
            properties.MaxDeliveryCount = MaxDeliveryCount ?? 10;
            properties.EnableBatchedOperations = EnableBatchedOperations ?? true;
            properties.ForwardTo = ForwardTo;
            properties.ForwardDeadLetteredMessagesTo = ForwardDeadLetteredMessagesTo;
        }
        
        // ----------------------- Public Interface ----------------------

        /// <summary>
        /// Called to add a rule to a subscription used to determine if messages apply.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="filter">Reference to the created filter.</param>
        /// <param name="config">Delete called used to apply additional settings.</param>
        public void AddRule(string name, RuleFilter filter, Action<CreateRuleOptions> config = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Rule name not specified.", nameof(name));
            
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            
            if (_rules.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"Subscription {SubscriptionName} already has a filter named {name} defined.");
            }
            
            var rule = _rules[name] = new CreateRuleOptions(name, filter);
            config?.Invoke(rule);
        }

        public override string ToString() => $"{NamespaceName}:{EntityName}:{UniqueSubscriptionName}";
    }
}