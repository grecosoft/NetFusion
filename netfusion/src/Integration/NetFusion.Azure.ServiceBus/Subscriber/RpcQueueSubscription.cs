using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Strategies;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    public class RpcQueueSubscription : EntitySubscription
    {
        private readonly IDictionary<string, MessageDispatchInfo> _messageNamespaceDispatchers;
        
        public RpcQueueSubscription(string namespaceName, string queueName, IEnumerable<MessageDispatchInfo> dispatchers)
            : base(namespaceName, queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(queueName));

            _messageNamespaceDispatchers = GetDispatchersByNamespace(dispatchers);
            
            SubscriptionStrategy = new RpcQueueSubscriptionStrategy(this);
            Options.MaxConcurrentCalls = 20;
            Options.PrefetchCount = 10;
            Options.ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete;
        }

        // Returns a dictionary used to determine the MessageDispatchInfo to use
        // for routing a RPC received command based on its assigned Message Namespace.
        private static IDictionary<string, MessageDispatchInfo> GetDispatchersByNamespace(
            IEnumerable<MessageDispatchInfo> dispatchers)
        {
            var rpcMessageDispatchers = dispatchers.Select(md => new
            {
                MessageNamespace = GetMessageNamespace(md),
                DispatchInfo = md
            }).ToArray();

            var invalidDispatchers = rpcMessageDispatchers.Where(d => d.MessageNamespace == null)
                .Select(d => d.DispatchInfo.ToString());
            
            if (invalidDispatchers.Any())
            {
                throw new ContainerException(
                    "One or more MessageNamespaces could not be determined for RPC message handler subscriptions.", 
                    "InvalidDispatchers", invalidDispatchers);
            }
            
            return rpcMessageDispatchers.ToDictionary(map => map.MessageNamespace, map => map.DispatchInfo);
        }

        private static string GetMessageNamespace(MessageDispatchInfo dispatchInfo)
        { 
            return dispatchInfo.MessageHandlerMethod.GetCustomAttribute<RpcQueueSubscriptionAttribute>()?.MessageNamespace
                   ?? dispatchInfo.MessageType.GetCustomAttribute<MessageNamespaceAttribute>()?.MessageNamespace;
        }

        internal MessageDispatchInfo GetMessageNamespaceDispatch(string messageNamespace)
        {
            if (string.IsNullOrWhiteSpace(messageNamespace))
                throw new ArgumentException($"Message Namespace not specified.", nameof(messageNamespace));

            return _messageNamespaceDispatchers.TryGetValue(messageNamespace, out var dispatchInfo) ? dispatchInfo : null;
        }

        public override string ToString() => $"{NamespaceName}:{EntityName}";
    }
}