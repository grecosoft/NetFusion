using System;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
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
    
    public class QueueSourceMeta<TCommand> : QueueSourceMeta,
        IMessageFilter
        where TCommand : ICommand
    {
        private Action<ServiceBusMessage, TCommand> _busMessageUpdateAction;
        private Func<TCommand, bool> _applies;
        
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
            
            _busMessageUpdateAction?.Invoke(busMessage, (TCommand)message);
        }
        
        /// <summary>
        /// Specifies a delegate called when a message is being published used 
        /// to set corresponding properties on the created service bus message.
        /// </summary>
        /// <param name="action">Passed the message being published and the
        /// associated created service bus message.</param>
        public void SetBusMessageProps(Action<ServiceBusMessage, TCommand> action)
        {
            _busMessageUpdateAction = action;
        }

        public void When(Func<TCommand, bool> applies)
        {
            _applies = applies ?? throw new ArgumentNullException(nameof(applies));
        }

        bool IMessageFilter.Applies(IMessage message)
        {
            return _applies?.Invoke((TCommand) message) ?? true;
        }
    }
}