using System;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    public abstract class QueueSourceMeta : NamespaceEntity
    {
        protected QueueSourceMeta(Type messageType, string namespaceName, string queueName) 
            : base(messageType, namespaceName, queueName)
        {
            EntityStrategy = new NoOpStrategy();
        }
        
        public string ReplyToQueueName { get; set; }
    }
    
    public class QueueSourceMeta<TCommand> : QueueSourceMeta
        where TCommand : ICommand
    {
        public QueueSourceMeta(string namespaceName, string queueName) 
            : base(typeof(TCommand), namespaceName, queueName)
        {
            
        }
        
        internal override void SetBusMessageProps(ServiceBusMessage busMessage, IMessage message)
        {
            if (!string.IsNullOrWhiteSpace(ReplyToQueueName))
            {
                busMessage.ReplyTo = $"{NamespaceName}:{ReplyToQueueName}";
            }
        }
    }
}