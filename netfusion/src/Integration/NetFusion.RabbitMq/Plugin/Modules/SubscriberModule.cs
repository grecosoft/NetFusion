using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Plugin.Modules
{
    /// <summary>
    /// Plugin module responsible for determining message handler methods that should be 
    /// subscribed to queues.  Any IMessageConsumer class method decorated with a derived 
    /// SubscriberQueue attribute is considered a handler that should be bound to a queue.
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        // Dependent Modules:
        protected IBusModule BusModule { get; set; }
        protected IMessageDispatchModule MessagingModule { get; set; }
        
        private ISerializationManager _serializationManager;
        private IMessageLogger _messageLogger;

        // Message handlers subscribed to queues:
        private MessageQueueSubscriber[] _subscribers = Array.Empty<MessageQueueSubscriber>();
        
        // ------------------------- [Plugin Execution] --------------------------
        
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            // Dependent Services:
            _serializationManager = services.GetRequiredService<ISerializationManager>();
            _messageLogger = services.GetRequiredService<IMessageLogger>();

            BusModule.Reconnection += async (sender, args) => await HandleReconnection(args);

            _subscribers = GetQueueSubscribers(MessagingModule);
            return SubscribeToQueues(BusModule, _subscribers);
        }

        protected override Task OnStopModuleAsync(IServiceProvider services)
        {
            _subscribers.ForEach(s => s.Consumer?.Dispose());
            return base.OnStopModuleAsync(services);
        }

        // Delegates to the core message dispatch module to find all message dispatch
        // handlers and filters the list to only those that should be bound to a queue.
        private MessageQueueSubscriber[] GetQueueSubscribers(IMessageDispatchModule messageDispatch)
        {
            var hostId = BusModule.HostAppId;
  
            return messageDispatch.AllMessageTypeDispatchers
                .Values().Where(MessageQueueSubscriber.IsSubscriber)
                .Select(d => new MessageQueueSubscriber(hostId, d))
                .ToArray();
        }
        
        // For each message handler identified as being associated with an exchange/queue, create the
        // exchange and queue then bind it to the in-process handler. 
        private async Task SubscribeToQueues(IBusModule busModule, IEnumerable<MessageQueueSubscriber> subscribers)
        {
            // Tracks the RPC queue that have been already bound.  For a given named RPC queue, we only
            // want to bind once since the command action namespace is used to determine the actual
            // handler to be called (multiple commands are sent on a single queue).
            HashSet<string> boundToRpcQueues = new HashSet<string>();
            
            foreach (var subscriber in subscribers)
            {
                busModule.ApplyQueueSettings(subscriber.QueueMeta);
 
                if (subscriber.QueueMeta.ExchangeMeta.IsRpcExchange 
                    && boundToRpcQueues.Contains(subscriber.QueueMeta.QueueName))
                {
                    continue;
                }
                
                var bus = busModule.GetBus(subscriber.QueueMeta.ExchangeMeta.BusName);
                IQueue queue = await QueueDeclareAsync(bus, subscriber.QueueMeta);
                
                ConsumeMessageQueue(bus, queue, subscriber);
                
                if (subscriber.QueueMeta.ExchangeMeta.IsRpcExchange)
                {
                    boundToRpcQueues.Add(queue.Name);
                }
            }
        }

        protected virtual Task<IQueue> QueueDeclareAsync(IBus bus, QueueMeta queueMeta)
        {
            return bus.Advanced.QueueDeclare(queueMeta);
        }

        // Defines a callback function to be called when a message arrives on the queue.
        protected virtual void ConsumeMessageQueue(IBus bus, IQueue queue, MessageQueueSubscriber subscriber)
        {
            QueueMeta definition = subscriber.QueueMeta;

            subscriber.Consumer = bus.Advanced.Consume(queue, 
                (bytes, msgProps, receiveInfo) => 
                {
                    // Create context containing the received message information and
                    // additional services required to process the message.
                    var consumerContext = new ConsumeContext(subscriber, msgProps, receiveInfo, bytes)
                    {
                        LoggerFactory = Context.LoggerFactory,
                        GetRpcMessageHandler = GetRpcMessageHandler,
          
                        // Dependent Services:
                        BusModule = BusModule,
                        MessagingModule = MessagingModule,
                        Serialization = _serializationManager,
                        MessageLogger = _messageLogger
                    };

                    // Delegate to the queue strategy, associated with the definition, and 
                    // allow it to determine how the received message should be processed. 
                    return definition.QueueStrategy.OnMessageReceivedAsync(consumerContext);
                }, 
                config => 
                {
                    if (definition.PrefetchCount > 0)
                    {
                        config.WithPrefetchCount(definition.PrefetchCount);
                    }

                    if (definition.IsExclusive)
                    {
                        config.WithExclusive();
                    }

                    config.WithPriority(definition.Priority);
                });
        }

        // Called when a reconnection to the the broker is detected.  The IBus will reestablish
        // the connection when available.  However, the reconnection could mean that the broker
        // was restarted.  In this case, any auto-delete queues will need to be recreated.
        private async Task HandleReconnection(ReconnectionEventArgs eventArgs)
        {
            var busSubscribers = _subscribers
                .Where(s => s.QueueMeta.ExchangeMeta.BusName == eventArgs.Connection.BusName)
                .ToArray();            
            
            busSubscribers.ForEach(s => s.Consumer?.Dispose());            
            await SubscribeToQueues(BusModule, busSubscribers);
        }

        // Looks up the dispatch information that should be used to handle the RPC style message.
        private MessageDispatchInfo GetRpcMessageHandler(string queueName, string actionNamespace)
        {
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (actionNamespace == null) throw new ArgumentNullException(nameof(actionNamespace));
            
            var matchingDispatchers = _subscribers.Where(s =>
                s.QueueMeta.QueueName == queueName &&
                s.QueueMeta.ExchangeMeta.ActionNamespace == actionNamespace).ToArray();

            if (matchingDispatchers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"A RPC command message handler could not be found for Queue: {queueName} with " + 
                    $"action namespace: {actionNamespace}");
            }

            if (matchingDispatchers.Length > 1)
            {
                throw new InvalidOperationException(
                    $"More than one RPC command message handler was found for Queue: {queueName} with " + 
                    $"action namespace: {actionNamespace}");
            }

            return matchingDispatchers.First().DispatchInfo;
        }
        
        // ------------------------ [Public Logging] --------------------------

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["SubscriberQueues"] = _subscribers.Select(s =>
            {
                var queueLog = new Dictionary<string, object>();
                s.QueueMeta.LogProperties(queueLog);
                queueLog["ConsumerType"] = s.DispatchInfo.ConsumerType.FullName;
                queueLog["MessageType"] = s.DispatchInfo.MessageType.FullName;
                queueLog["HandlerMethod"] = s.DispatchInfo.MessageHandlerMethod.Name;

                return queueLog;
            }).ToArray();
        }
    }
}