using System;
using System.Reflection;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    public abstract class RpcQueueSourceMeta : NamespaceEntity
    {
        /// <summary>
        /// Namespace identifying the command message.  Used by the subscriber to
        /// how the command is to be routed.
        /// </summary>
        internal string MessageNamespace { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        internal string ReplyQueueName { get; set; }

        public int CancelRpcRequestAfterMs { get; set; } = 10_000;
        
        protected RpcQueueSourceMeta(Type messageType, string namespaceName, string queueName) 
            : base(messageType, namespaceName, queueName)
        {
            EntityStrategy = new RpcQueueStrategy(this);
        }
    }

    public class RpcQueueSourceMeta<TCommand> : RpcQueueSourceMeta
        where TCommand : ICommand
    {
        public RpcQueueSourceMeta(string namespaceName, string queueName,
            string messageNamespace)
            : base(typeof(TCommand), namespaceName, queueName)
        {
            MessageNamespace = messageNamespace;
            ReplyQueueName = $"rpc_{queueName}_{Guid.NewGuid()}";
        }
        
        internal override void SetBusMessageProps(ServiceBusMessage busMessage, IMessage message)
        {
            busMessage.ReplyTo = $"{NamespaceName}:{ReplyQueueName}";
            
            busMessage.ApplicationProperties["MessageNamespace"] = message.GetType()
                .GetCustomAttribute<MessageNamespaceAttribute>().MessageNamespace;
        }
    }
}