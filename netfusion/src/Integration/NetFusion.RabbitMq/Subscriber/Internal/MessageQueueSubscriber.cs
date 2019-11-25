using System;
using System.Reflection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Internal;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Stores an association between a method that is to be dispatched
    /// when a message is delivered to a queue on a specific exchange.
    /// </summary>
    public class MessageQueueSubscriber
    {
        public MessageDispatchInfo DispatchInfo { get; }
        public QueueMeta QueueMeta { get; }
        private readonly string _hostId;
  
        public MessageQueueSubscriber(string hostId, MessageDispatchInfo dispatchInfo)
        {
            if (string.IsNullOrWhiteSpace(hostId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(hostId));
            
            if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));

            _hostId = hostId;
            
            // Obtain the subscriber attribute so the definition metadata
            // can be retrieved.
            var queueAttribute = dispatchInfo.MessageHandlerMethod
                .GetCustomAttribute<SubscriberQueueAttribute>();

            DispatchInfo = dispatchInfo;
            QueueMeta = queueAttribute.QueueStrategy.CreateQueueMeta(queueAttribute);
            QueueMeta.QueueStrategy = queueAttribute.QueueStrategy;
            
            ApplyScopedQueueName(QueueMeta);
        }

        // Any dispatch corresponding to a method decorated with a derived
        // SubscriberQueue attribute will be bound to a queue.
        public static bool IsSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null)
                throw new ArgumentNullException(nameof(dispatchInfo));

            return dispatchInfo.MessageHandlerMethod.HasAttribute<SubscriberQueueAttribute>();
        }   
        
        private void ApplyScopedQueueName(QueueMeta meta)
        {
            if (meta.AppendHostId)
            {
                meta.SetScopedName($"{meta.QueueName}->({_hostId})");
            }

            if (meta.AppendUniqueId)
            {
                meta.SetScopedName($"{meta.QueueName}<-({Guid.NewGuid()})");
            }
        }
    }
}