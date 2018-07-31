using IMessage = NetFusion.Messaging.Types.IMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.RabbitMQ.Subscriber.Internal;
using NetFusion.RabbitMQ.Settings;
using NetFusion.Bootstrap.Logging;

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plugin module responsible for determining message handler methods that should be 
    /// subscribed to queues.  Any IMessageConsumer method decorated with a derived 
    /// SubscriberQueue attribute is considered a handler that should be bound to a queue.
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        // Dependent services:
        private IBusModule _busModule;
        private IMessageDispatchModule _messagingModule;
        private ISerializationManager _serializationManager;

        // Message handlers subscribed to queues:
        private MessageQueueSubscriber[] _subscribers;

        public override void StartModule(IServiceProvider services)
        {
            // Dependent Services:
            _busModule = services.GetRequiredService<IBusModule>();
            _messagingModule = services.GetRequiredService<IMessageDispatchModule>();
            _serializationManager = services.GetRequiredService<ISerializationManager>();

            _subscribers = GetQueueSubscribers(_messagingModule);
            SubscribeToQueues(_busModule, _subscribers);
        } 

        // Delegates to the core message dispatch module to find all message dispatch
        // handlers and filters the list to only those that should be bound to a queue.
        private static MessageQueueSubscriber[] GetQueueSubscribers(IMessageDispatchModule messsageDispatch)
        {
            return messsageDispatch.AllMessageTypeDispatchers
                .Values().Where(MessageQueueSubscriber.IsSubscriber)
                .Select(d => new MessageQueueSubscriber(d))
                .ToArray();
        }

        // For each message handler identified as being associated with an exchange/queue, create the
        // exchange and queue then bind it to the in-process handler. 
        private void SubscribeToQueues(IBusModule busModule, IEnumerable<MessageQueueSubscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                var bus = busModule.GetBus(subscriber.ExchangeDefinition.BusName);
                var exchange = CreateExchange(bus, subscriber);
                var queue = CreateQueue(bus, exchange, subscriber);  

                ConsumeMessageQueue(bus, queue, subscriber);
            }
        }

        private IExchange CreateExchange(IBus bus, MessageQueueSubscriber subscriber)
        {
            QueueExchangeDefinition definition = subscriber.ExchangeDefinition;

            // This is the case where the queue is defined on Rabbit's default exchange.
            if (definition.IsDefaultExchange)
            {
                return Exchange.GetDefault();
            }

            // If there are externally configured exchange settings, override the default conventions:
            ExchangeSettings exchangeSettings = _busModule.GetExchangeSettings(definition.BusName, definition.ExchangeName);
            if (exchangeSettings != null)
            {
                definition.ApplyOverrides(exchangeSettings);
            }

            return CreateExchange(bus, definition);
        }

        protected virtual IExchange CreateExchange(IBus bus, QueueExchangeDefinition definition)
        {
            return bus.Advanced.ExchangeDeclare(definition.ExchangeName, definition.ExchangeType,
                definition.IsPassive,
                definition.IsDurable,
                definition.IsAutoDelete,
                alternateExchange: definition.AlternateExchange);
        }

        private IQueue CreateQueue(IBus bus, IExchange exchange, MessageQueueSubscriber subscriber)
        {
            QueueDefinition queueDefinition = subscriber.QueueDefinition;
            QueueSettings queueSettings = _busModule.GetQueueSettings(subscriber.ExchangeDefinition.BusName, queueDefinition.QueueName);

            // If there are externally configured queue settings, override the default conventions:
            if (queueSettings != null)
            {
                queueDefinition.ApplyOverrides(queueSettings);
            } 

            var queueContext = new QueueContext 
            {
                Bus = bus,
                Exchange = exchange,
                Definition = subscriber.QueueDefinition,
                HostId = Context.AppHost.Manifest.PluginId
            };

            return CreateQueue(subscriber, queueContext);
        }

        protected virtual IQueue CreateQueue(MessageQueueSubscriber subscriber, QueueContext queueContext)
        {
            // Delegate to the definition to build the specific type of queue:
            return subscriber.QueueDefinition.CreateQueue(queueContext);  
        }

        // Defines a callback function to be called when a message arrives on the queue.
        protected virtual void ConsumeMessageQueue(IBus bus, IQueue queue, MessageQueueSubscriber subscriber)
        {
                QueueDefinition definition = subscriber.QueueDefinition;

                // Subscribe to the queue, and based on the content type, deserialize the received
                // bytes into the message type associated with the in-process handler and invoke. 
                bus.Advanced.Consume(queue, 
                    (bytes, msgProps, receiveInfo) => 
                    {
                        IMessage message = DeserializeIntoMessage(subscriber, bytes, msgProps.ContentType);
                        LogReceivedMessage(subscriber, msgProps, message);

                        var consumerContext = new ConsumeContext 
                        {
                            MessageProps = msgProps,
                            MessageReceiveInfo = receiveInfo,
                            Subscriber = subscriber,
                            BusModule = _busModule,
                            MessagingModule = _messagingModule,
                            Serialization = _serializationManager
                        };

                        // Delegate to the queue factory, associated with the definition, and 
                        // allow it to determine how the received messaged should be processed. 
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
                    Bus = subscriber.ExchangeDefinition.BusName,
                    Exchange = subscriber.ExchangeDefinition.ExchangeName,
                    Queue = subscriber.QueueDefinition.QueueName,
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
            moduleLog["Queue-Subscriptions"] = _subscribers.Select(s => new {
                ConsumerType = s.DispatchInfo.ConsumerType.AssemblyQualifiedName,
                MessageType = s.DispatchInfo.MessageType.AssemblyQualifiedName,
                HandlerMethod = s.DispatchInfo.MessageHandlerMethod.Name,
                s.ExchangeDefinition,
                s.QueueDefinition
            }).ToArray();
        }
    }
}