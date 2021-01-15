using System;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    /// <summary>
    /// Non-generic version of the class used by the base code implementation.
    /// </summary>
    public class QueueMeta : NamespaceEntity
    {
        public QueueMeta(Type messageType, string namespaceName, string queueName)
            : base(messageType, namespaceName, queueName)
        {
            EntityStrategy = new QueueStrategy(this);
        }

        /// <summary>
        /// Duration of a peek lock receive. i.e., the amount of time that the message is locked by a given receiver so that
        /// no other receiver receives the same message.
        /// </summary>
        public TimeSpan? LockDuration { get; set; }
        
        /// <summary>
        /// This indicates whether the queue supports the concept of session. Sessionful-messages follow FIFO ordering.
        /// </summary>
        public bool? RequiresSession { get; set; }
        
        /// <summary>
        /// Indicates whether this queue has dead letter support when a message expires.
        /// </summary>
        public bool? DeadLetteringOnMessageExpiration { get; set; }
        
        /// <summary>
        /// The maximum delivery count of a message before it is dead-lettered.
        /// </summary>
        public int? MaxDeliveryCount { get; set; }
        
        /// <summary>
        /// The name of the recipient entity to which all the messages sent to the queue are forwarded to.
        /// </summary>
        public string ForwardTo { get; set; }
        
        /// <summary>
        /// The name of the recipient entity to which all the dead-lettered messages of this queue are forwarded to.
        /// </summary>
        public string ForwardDeadLetteredMessagesTo { get; set; }
        
        /// <summary>
        /// The maximum size of the queue in megabytes, which is the size of memory allocated for the queue.
        /// </summary>
        public long? MaxSizeInMegabytes { get; set; }
        
        /// <summary>
        /// This value indicates if the queue requires guard against duplicate messages. If true, duplicate messages having same
        /// <see cref="ServiceBusMessage.MessageId"/> and sent to queue within duration of <see cref="DuplicateDetectionHistoryTimeWindow"/>
        /// will be discarded.
        /// </summary>
        public bool? RequiresDuplicateDetection { get; set; }
        
        /// <summary>
        /// The default time to live value for the messages. This is the duration after which the message expires, starting from when
        /// the message is sent to Service Bus.
        /// </summary>
        public TimeSpan? DefaultMessageTimeToLive { get; set; }
        
        /// <summary>
        /// The <see cref="TimeSpan"/> idle interval after which the queue is automatically deleted.
        /// </summary>
        public TimeSpan? AutoDeleteOnIdle { get; set; }
        
        /// <summary>
        /// The <see cref="TimeSpan"/> duration of duplicate detection history that is maintained by the service.
        /// </summary>
        public TimeSpan? DuplicateDetectionHistoryTimeWindow { get; set; }
        
        /// <summary>
        /// Indicates whether server-side batched operations are enabled.
        /// </summary>
        public bool? EnableBatchedOperations { get; set; }
        
        /// <summary>
        /// Indicates whether the queue is to be partitioned across multiple message brokers.
        /// </summary>
        public bool? EnablePartitioning { get; set; }

        internal CreateQueueOptions ToCreateOptions()
        {
            return new CreateQueueOptions(EntityName)
            {
                LockDuration = LockDuration ?? TimeSpan.FromSeconds(60),
                RequiresSession = RequiresSession ?? false,
                DeadLetteringOnMessageExpiration = DeadLetteringOnMessageExpiration ?? false,
                MaxDeliveryCount = MaxDeliveryCount ?? 10,
                ForwardTo = ForwardTo,
                ForwardDeadLetteredMessagesTo = ForwardDeadLetteredMessagesTo,
                MaxSizeInMegabytes = MaxSizeInMegabytes ?? 1024,
                RequiresDuplicateDetection = RequiresDuplicateDetection ?? false,
                DefaultMessageTimeToLive = DefaultMessageTimeToLive ?? TimeSpan.MaxValue,
                AutoDeleteOnIdle = AutoDeleteOnIdle ?? TimeSpan.MaxValue,
                DuplicateDetectionHistoryTimeWindow = DuplicateDetectionHistoryTimeWindow ?? TimeSpan.FromMinutes(1),
                EnableBatchedOperations = EnableBatchedOperations ?? true,
                EnablePartitioning = EnablePartitioning ?? false,
            };
        }

        internal void UpdateProperties(QueueProperties properties)
        {
            properties.LockDuration = LockDuration ?? TimeSpan.FromSeconds(60);
            properties.DeadLetteringOnMessageExpiration = DeadLetteringOnMessageExpiration ?? false;
            properties.MaxDeliveryCount = MaxDeliveryCount ?? 10;
            properties.ForwardTo = ForwardTo;
            properties.ForwardDeadLetteredMessagesTo = ForwardDeadLetteredMessagesTo;
            properties.MaxSizeInMegabytes = MaxSizeInMegabytes ?? 1024;
            properties.DefaultMessageTimeToLive = DefaultMessageTimeToLive ?? TimeSpan.MaxValue;
            properties.AutoDeleteOnIdle = AutoDeleteOnIdle ?? TimeSpan.MaxValue;
            properties.DuplicateDetectionHistoryTimeWindow = DuplicateDetectionHistoryTimeWindow ?? TimeSpan.FromMinutes(1);
            properties.EnableBatchedOperations = EnableBatchedOperations ?? true;
        }
    }

    /// <summary>
    /// Specifies a queue associated with a message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class QueueMeta<TMessage> : QueueMeta
        where TMessage : IMessage
    {
        public QueueMeta(string namespaceName, string queueName) 
            : base(typeof(TMessage), namespaceName, queueName)
        {
            
        }
    }
}