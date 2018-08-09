using IMessage = NetFusion.Messaging.Types.IMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.RabbitMQ.Subscriber.Internal;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plugin module responsible for determining message handler methods that should be 
    /// subscribed to queues.  Any IMessageConsumer class method decorated with a derived 
    /// SubscriberQueue attribute is considered a handler that should be bound to a queue.
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        // Dependent modules:
        private IBusModule _busModule;
        private IMessageDispatchModule _messagingModule;
        private ISerializationManager _serializationManager;

        // Message handlers subscribed to queues:
        private MessageQueueSubscriber[] _subscribers;

        public override void StartModule(IServiceProvider services)
        {
            // Dependent modules:
            _busModule = services.GetRequiredService<IBusModule>();
            _messagingModule = services.GetRequiredService<IMessageDispatchModule>();
            _serializationManager = services.GetRequiredService<ISerializationManager>();

            _subscribers = GetQueueSubscribers(_messagingModule);
            SubscribeToQueues(_busModule, _subscribers);
        }
        
        // Delegates to the core message dispatch module to find all message dispatch
        // handlers and filters the list to only those that should be bound to a queue.
        private MessageQueueSubscriber[] GetQueueSubscribers(IMessageDispatchModule messsageDispatch)
        {
            var hostId = _busModule.HostAppId;
  
            return messsageDispatch.AllMessageTypeDispatchers
                .Values().Where(MessageQueueSubscriber.IsSubscriber)
                .Select(d => new MessageQueueSubscriber(hostId, d))
                .ToArray();
        }

        // Looks up the dispach information that should be used to handle the RPC style message.
        private MessageDispatchInfo GetRpcMessageHandler(string queueName, string actionNamespace)
        {
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));
            if (actionNamespace == null) throw new ArgumentNullException(nameof(actionNamespace));
            
            var matchingDispatchers = _subscribers.Where(s =>
                s.QueueMeta.QueueName == queueName &&
                s.QueueMeta.Exchange.ActionNamespace == actionNamespace).ToArray();

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
        
        // For each message handler identified as being associated with an exchange/queue, create the
        // exchange and queue then bind it to the in-process handler. 
        private void SubscribeToQueues(IBusModule busModule, IEnumerable<MessageQueueSubscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                _busModule.ApplyQueueSettings(subscriber.QueueMeta);
                
                var bus = busModule.GetBus(subscriber.QueueMeta.Exchange.BusName);
                IQueue queue = bus.Advanced.QueueDeclare(subscriber.QueueMeta);
             
                ConsumeMessageQueue(bus, queue, subscriber);
            }
        }

        // Defines a callback function to be called when a message arrives on the queue.
        protected virtual void ConsumeMessageQueue(IBus bus, IQueue queue, MessageQueueSubscriber subscriber)
        {
                QueueMeta definition = subscriber.QueueMeta;

                // Subscribe to the queue, and based on the content type, deserialize the received
                // bytes into the message type associated with the in-process handler and invoke. 
                bus.Advanced.Consume(queue, 
                    (bytes, msgProps, receiveInfo) => 
                    {
                        IMessage message = DeserializeIntoMessage(subscriber, bytes, msgProps.ContentType);
                        LogReceivedMessage(subscriber, msgProps, message);

                        var consumerContext = new ConsumeContext 
                        {
                            Logger = Context.LoggerFactory.CreateLogger(definition.QueueFactory.GetType().FullName),
                            MessageProps = msgProps,
                            MessageReceiveInfo = receiveInfo,
                            Subscriber = subscriber,
                            BusModule = _busModule,
                            MessagingModule = _messagingModule,
                            Serialization = _serializationManager,
                            GetRpcMessageHandler = GetRpcMessageHandler
                        };

                        // Delegate to the queue factory, associated with the definition, and 
                        // allow it to determine how the received message should be dispatched. 
                        return definition.QueueFactory.OnMessageReceived(consumerContext, message);
                    }, 
                    config => 
                    {
                        if (definition.PrefetchCount > 0)
                        {
                            config.WithPrefetchCount(definition.PrefetchCount);
                        }

                        if (definition.IsExclusive)
                        {
                            config.AsExclusive();
                        }

                        config.WithPriority(definition.Priority);
                    });
        }

        private void LogReceivedMessage(MessageQueueSubscriber subscriber, MessageProperties msgProps,
            IMessage message)
        {
            Context.Logger.LogTraceDetails(RabbitMqLogEvents.SubscriberEvent, 
                "Message Received from Message Bus.", 
                new {
                    Bus = subscriber.QueueMeta.Exchange.BusName,
                    Exchange = subscriber.QueueMeta.Exchange.ExchangeName,
                    Queue = subscriber.QueueMeta.QueueName,
                    msgProps.ContentType,
                    Consumer = subscriber.DispatchInfo.ConsumerType.Name,
                    Handler = subscriber.DispatchInfo.MessageHandlerMethod.Name,
                    Message = message
                });
        }

        private IMessage DeserializeIntoMessage(MessageQueueSubscriber subscriber, byte[] bytes, string contentType)
        {
            Type messageType = subscriber.DispatchInfo.MessageType;
            object message = _serializationManager.Deserialize(contentType, messageType, bytes);
            return (IMessage)message;
        }
       
        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Subscriber:Queues"] = _subscribers.Select(s =>
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