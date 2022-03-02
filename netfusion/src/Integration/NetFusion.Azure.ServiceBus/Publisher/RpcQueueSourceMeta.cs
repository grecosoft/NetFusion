using System;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    /// <summary>
    /// Specifies an existing RPC style queue, defined by another microservice, to which a command
    /// should be delivered when published.  While the response is asynchronous and received on a 
    /// reply queue, the caller awaits for the response to be received.
    /// </summary>
    public abstract class RpcQueueSourceMeta : NamespaceEntity
    {
        public const int DefaultCancelRpcRequestAfterMs = 10_000;

        /// <summary>
        /// Namespace identifying the command message.  Used by the subscriber to route the command 
        /// to the correct handler method.
        /// </summary>
        internal string MessageNamespace { get; set; }

        /// <summary>
        /// The number of milliseconds after sending the command the request should be canceled if 
        /// a response is not received.  The default value is 10 seconds.
        /// </summary>
        public int CancelRpcRequestAfterMs { get; set; } = DefaultCancelRpcRequestAfterMs;

        /// <summary>
        /// The corresponding queue on which RPC reply messages are received.
        /// </summary>
        internal RpcReplyQueryMeta ReplyQueue { get; set; }
        
        protected RpcQueueSourceMeta(Type messageType, string messageNamespace, RpcReplyQueryMeta rpcReplyQueue) 
            : base(rpcReplyQueue.NamespaceName, rpcReplyQueue.EntityName, messageType)
        {
            if (string.IsNullOrWhiteSpace(messageNamespace))
            {
                throw new ArgumentException("Cannot be null or whitespace.", nameof(messageNamespace));
            }

            EntityStrategy = new RpcQueueEntityStrategy(this);
            MessageNamespace = messageNamespace;
            ReplyQueue = rpcReplyQueue ?? throw new ArgumentNullException(nameof(rpcReplyQueue));
        }
    }

    /// <summary>
    /// Specifies an existing RPC style queue, defined by another microservice, to which a command
    /// should be delivered when published.  While the response is asynchronous and received on a 
    /// reply queue, the caller awaits for the response to be received.
    /// </summary>
    public class RpcQueueSourceMeta<TCommand> : RpcQueueSourceMeta
        where TCommand : ICommand
    {
        public RpcQueueSourceMeta(string messageNamespace, RpcReplyQueryMeta rpcReplyQueue)
            : base(typeof(TCommand), messageNamespace, rpcReplyQueue)
        {
 
        }
        
        internal override void SetBusMessageProps(ServiceBusMessage busMessage, IMessage message)
        {
            busMessage.ReplyTo = $"{NamespaceName}:{ReplyQueue.UniqueReplyQueueName}";
            busMessage.ApplicationProperties["MessageNamespace"] = MessageNamespace;
        }
    }
}