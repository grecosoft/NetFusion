using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Strategies;
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

        private static IDictionary<string, MessageDispatchInfo> GetDispatchersByNamespace(
            IEnumerable<MessageDispatchInfo> dispatchers)
        {
            return dispatchers.Select(md => new
                {
                    Namespace = GetMessageNamespace(md),
                    DispatchInfo = md
                }).Where(map => map.Namespace != null)
                .ToDictionary(map => map.Namespace, map => map.DispatchInfo);
        }

        private static string GetMessageNamespace(MessageDispatchInfo dispatchInfo)
        {
            return dispatchInfo.MessageHandlerMethod.GetCustomAttribute<RpcQueueSubscriptionAttribute>()?.MessageNamespace
                   ?? dispatchInfo.MessageType.GetCustomAttribute<MessageNamespaceAttribute>()?.MessageNamespace;
        }

        internal MessageDispatchInfo GetMessageNamespaceDispatch(string messageNamespace)
        {
            return _messageNamespaceDispatchers.TryGetValue(messageNamespace, out var dispatchInfo) ? dispatchInfo : null;
        }

        public override string ToString() => $"{NamespaceName}:{EntityName}";
    }
}