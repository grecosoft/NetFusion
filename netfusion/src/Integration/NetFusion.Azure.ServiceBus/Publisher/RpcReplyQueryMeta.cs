using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Strategies;
using System;
using System.Collections.Concurrent;

namespace NetFusion.Azure.ServiceBus.Publisher
{
    /// <summary>
    /// Represents a reply queue associated with a RpcQueueSourceMeta on which
    /// RPC reply messages are received.
    /// </summary>
    public class RpcReplyQueryMeta : NamespaceEntity
    {
        /// <summary>
        /// Unique queue name, based on the RPC Queue name to which the command 
        /// is sent, on which replay message are received.
        /// </summary>
        internal string UniqueReplyQueueName { get; set; }

        /// <summary>
        /// Dictionary containing the pending tasks associated with the outgoing
        /// RPC command keyed by Message Id.  
        /// </summary>
        internal ConcurrentDictionary<string, RpcPendingRequest> PendingRpcRequests { get; }


        public RpcReplyQueryMeta(string namespaceName, string queueName) 
            : base(namespaceName, queueName, typeof(void))
        {
            PendingRpcRequests = new ConcurrentDictionary<string, RpcPendingRequest>();
            EntityStrategy = new RpcReplyQueueEntityStrategy(this);

            UniqueReplyQueueName = $"rpc_{queueName}_{Guid.NewGuid()}";
        }
    }
}
