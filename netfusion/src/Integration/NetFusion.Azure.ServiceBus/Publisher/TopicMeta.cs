using System;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Amqp.Framing;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    /// <summary>
    /// Contains properties used to specify characteristics of a topic to be created.
    /// Also provides reference to the corresponding IEntityCreationStrategy.
    /// </summary>
    public abstract class TopicMeta : NamespaceEntity
    {
        protected TopicMeta(Type messageType, string namespaceName, string topicName)
            : base(messageType, namespaceName, topicName)
        {
            EntityStrategy = new TopicStrategy(this);
        }
        
        /// <summary>
        /// The maximum size of the topic in megabytes, which is the size of memory allocated for the topic.
        /// </summary>
        public long? MaxSizeInMegabytes { get; set; }
        
        /// <summary>
        /// This value indicates if the topic requires guard against duplicate messages. If true, duplicate messages having same
        /// <see cref="MessageId"/> and sent to topic within duration of <see cref="DuplicateDetectionHistoryTimeWindow"/>
        /// will be discarded.
        /// </summary>
        public bool? RequiresDuplicateDetection { get; set; }
        
        /// <summary>
        /// The default time to live value for the messages. This is the duration after which the message expires,
        /// starting from when the message is sent to Service Bus.
        /// </summary>
        public TimeSpan? DefaultMessageTimeToLive { get; set; }
        
        /// <summary>
        /// The <see cref="TimeSpan"/> idle interval after which the topic is automatically deleted.
        /// </summary>
        public TimeSpan? AutoDeleteOnIdle { get; set; }
        
        /// <summary>
        /// The <see cref="TimeSpan"/> duration of duplicate detection history that is maintained by the service.
        /// </summary>
        public TimeSpan? DuplicateDetectionHistoryTimeWindow { get; set; }
        
        /// <summary>
        /// Defines whether ordering needs to be maintained. If true, messages sent to topic will be
        /// forwarded to the subscription in order.
        /// </summary>
        public bool? SupportOrdering { get; set; }
        
        /// <summary>
        /// Indicates whether server-side batched operations are enabled.
        /// </summary>
        public bool? EnableBatchedOperations { get; set; }
        
        /// <summary>
        /// Indicates whether the topic is to be partitioned across multiple message brokers.
        /// </summary>
        public bool? EnablePartitioning { get; set; }
        
        internal CreateTopicOptions ToCreateOptions()
        {
            return new(EntityName)
            {
                MaxSizeInMegabytes = MaxSizeInMegabytes ?? 1024,
                RequiresDuplicateDetection = RequiresDuplicateDetection ?? false,
                DefaultMessageTimeToLive = DefaultMessageTimeToLive ?? TimeSpan.MaxValue,
                AutoDeleteOnIdle = AutoDeleteOnIdle ?? TimeSpan.MaxValue,
                DuplicateDetectionHistoryTimeWindow = DuplicateDetectionHistoryTimeWindow ?? TimeSpan.FromMinutes(1),
                EnableBatchedOperations = EnableBatchedOperations ?? true,
                EnablePartitioning = EnablePartitioning ?? false,
                SupportOrdering = SupportOrdering ?? false
            };
        }

        // Updates existing topic's properties so they can be reapplied. 
        internal void UpdateProperties(TopicProperties properties)
        {
            properties.MaxSizeInMegabytes = MaxSizeInMegabytes ?? 1024;
            properties.RequiresDuplicateDetection = RequiresDuplicateDetection ?? false;
            properties.DefaultMessageTimeToLive = DefaultMessageTimeToLive ?? TimeSpan.MaxValue;
            properties.AutoDeleteOnIdle = AutoDeleteOnIdle ?? TimeSpan.MaxValue;
            properties.DuplicateDetectionHistoryTimeWindow = DuplicateDetectionHistoryTimeWindow ?? TimeSpan.FromMinutes(1);
            properties.EnableBatchedOperations = EnableBatchedOperations ?? true;
            properties.EnablePartitioning = EnablePartitioning ?? false;
            properties.SupportOrdering = SupportOrdering ?? false;
        }
    }

    /// <summary>
    /// Contains properties defining how a Azure Service Bus topic should be created.
    /// </summary>
    /// <typeparam name="TDomainEvent">The Domain Event associated with the topic.</typeparam>
    public class TopicMeta<TDomainEvent> : TopicMeta,
        IMessageFilter
        where TDomainEvent : IDomainEvent
    {
        private Action<ServiceBusMessage, TDomainEvent> _busMessageUpdateAction;
        private Func<TDomainEvent, bool> _applies;
        
        public TopicMeta(string namespaceName, string name) 
            : base(typeof(TDomainEvent), namespaceName, name)
        {
            
        }

        internal override void SetBusMessageProps(ServiceBusMessage busMessage, IMessage message)
        {
            _busMessageUpdateAction?.Invoke(busMessage, (TDomainEvent)message);
        }

        /// <summary>
        /// Specifies a delegate called when a message is being published used 
        /// to set corresponding properties on the created service bus message.
        /// </summary>
        /// <param name="action">Passed the message being published and the
        /// associated created service bus message.</param>
        public void SetBusMessageProps(Action<ServiceBusMessage, TDomainEvent> action)
        {
            _busMessageUpdateAction = action ?? throw new ArgumentNullException(nameof(action));
        }
        
        public void When(Func<TDomainEvent, bool> applies)
        {
            _applies = applies ?? throw new ArgumentNullException(nameof(applies));
        }

        bool IMessageFilter.Applies(IMessage message)
        {
            return _applies?.Invoke((TDomainEvent) message) ?? true;
        }
    }
}