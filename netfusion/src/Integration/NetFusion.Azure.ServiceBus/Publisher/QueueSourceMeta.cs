using System;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    /// <summary>
    /// Specifies an existing queue to which a command messaged should
    /// be delivered when published.
    /// </summary>
    public abstract class QueueSourceMeta : NamespaceEntity
    {
        protected QueueSourceMeta(string namespaceName, string queueName, Type messageType)
            : base(namespaceName, queueName, messageType)
        {
            EntityStrategy = new NoOpEntityStrategy();
        }
        
        /// <summary>
        /// The optional queue name, defined by the publisher, on which it receives 
        /// asynchronous replies to the original published commands.
        /// </summary>
        public string ReplyToQueueName { get; set; }
    }

    /// <summary>
    /// Specifies an existing queue to which a command messaged  should
    /// be delivered when published.
    /// </summary>
    public class QueueSourceMeta<TCommand> : QueueSourceMeta
        where TCommand : ICommand
    {
        public QueueSourceMeta(string namespaceName, string queueName) 
            : base(namespaceName, queueName, typeof(TCommand))
        {
            
        }
        
        internal override void SetBusMessageProps(ServiceBusMessage busMessage, IMessage _)
        {
            if (!string.IsNullOrWhiteSpace(ReplyToQueueName))
            {
                busMessage.ReplyTo = $"{NamespaceName}:{ReplyToQueueName}";
            }
        }
    }
}