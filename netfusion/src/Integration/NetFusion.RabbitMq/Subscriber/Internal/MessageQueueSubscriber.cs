using System;
using System.Reflection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Core;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Stores an association between a method that is to be dispatched
    /// when a message is delivered to a queue on a specific exchange.
    /// </summary>
    public class MessageQueueSubscriber
    {
        public MessageDispatchInfo DispatchInfo { get; }
        public QueueDefinition QueueDefinition { get; }
        public QueueExchangeDefinition ExchangeDefinition { get; }

        public MessageQueueSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));
            
            // Obtain the subscriber attribute so the definition metadata
            // can be retreived.
            var queueAttrib = dispatchInfo.MessageHandlerMethod
                .GetCustomAttribute<SubscriberQueueAttribute>();
            
            DispatchInfo = dispatchInfo;
            QueueDefinition = queueAttrib.QueueDefinition;
            ExchangeDefinition = queueAttrib.ExchangeDefinition;

            SetDefaultConventions();
        }

        private void SetDefaultConventions()
        {
            QueueDefinition.QueueFactory.SetExchangeDefaults(ExchangeDefinition);
            QueueDefinition.QueueFactory.SetQueueDefaults(QueueDefinition);
        }

        // Any dispatch corresponding to a method decorated with a derived
        // SubscriberQueue attribute will be bound to a queue.
        public static bool IsSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null)
                throw new ArgumentNullException(nameof(dispatchInfo));

            return dispatchInfo.MessageHandlerMethod.HasAttribute<SubscriberQueueAttribute>();
        }   
    }
}