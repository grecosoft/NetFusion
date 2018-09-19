using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using NetFusion.Redis.Internal;
using NetFusion.Redis.Subscriber;
using NetFusion.Redis.Subscriber.Internal;
using StackExchange.Redis;

namespace NetFusion.Redis.Modules
{
    /// <summary>
    /// Module that determines all domain-event handlers that should be subscribed to 
    /// a Redis channel.  When a message is received on the channel, the associated
    /// event-handler is invoked with the deserialized domain-event instance.
    ///
    /// https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.modules#bootstrapping---modules
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        // Dependent Modules:
        private IConnectionModule _connModule;
        private IMessageDispatchModule _messagingModule;
        private ISerializationManager _serializationManager;
        
        // Message handlers subscribed to channels:
        private MessageChannelSubscriber[] _subscribers;

        // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.modules#registerdefaultservices
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ISubscriptionService, SubscriptionService>();
        }

        // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.modules#startmodule
        public override void StartModule(IServiceProvider services)
        {
            // Dependent modules:
            _connModule = services.GetRequiredService<IConnectionModule>();
            _messagingModule = services.GetRequiredService<IMessageDispatchModule>();
            _serializationManager = services.GetRequiredService<ISerializationManager>();

            _subscribers = GetChannelSubscribers(_messagingModule);
            SubscribeToChannels(_connModule, _subscribers);
        }
        
        // Delegates to the core message dispatch module to find all message dispatch
        // handlers and filters the list to only those that should be bound to a channel.
        private static MessageChannelSubscriber[] GetChannelSubscribers(IMessageDispatchModule messsageDispatch)
        {
            return messsageDispatch.AllMessageTypeDispatchers
                .Values().Where(MessageChannelSubscriber.IsSubscriber)
                .Select(d => new MessageChannelSubscriber(d))
                .ToArray();
        }
        
        private void SubscribeToChannels(IConnectionModule connModule, IEnumerable<MessageChannelSubscriber> subscribers)
        {
            foreach (var msgSubscriber in subscribers)
            {
                ISubscriber subscriber = connModule.GetSubscriber(msgSubscriber.DatabaseName);
                
                // Callback invoked when message published to channel:
                subscriber.Subscribe(msgSubscriber.Channel, (channel, message) =>
                {
                    Type messageType = msgSubscriber.DispatchInfo.MessageType;
                    var messageParts = ChannelMessage.UnPack(message);
                    
                    // Deserialize message byte array into domain-event type associated with handler:
                    IDomainEvent domainEvent = (IDomainEvent)_serializationManager.Deserialize(
                        messageParts.contentType, 
                        messageType, 
                        messageParts.messageData);

                    LogReceivedDomainEvent(channel, domainEvent, msgSubscriber);
                    
                    // Invoke the in-process handler:
                    _messagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                        msgSubscriber.DispatchInfo, 
                        domainEvent).Wait();
                });
            }
        }

        private void LogReceivedDomainEvent(string channel, IDomainEvent domainEvent,
            MessageChannelSubscriber subscriber)
        {
            Context.Logger.LogTraceDetails(RedisLogEvents.SubscriberEvent, 
                "Domain event received on Redis Channel.", 
                new
                {
                    Channel = channel,
                    Subscription = subscriber.Channel,
                    DomainEvent = domainEvent
                });
        }

        // https://github.com/grecosoft/NetFusion/wiki/core.logging.composite#module-logging
        public override void Log(IDictionary<string, object> moduleLog)
        {
            _subscribers.Select(s => new
            {
                s.Channel,
                s.DatabaseName,

                DispatchInfo = new
                {
                    ConsumerType = s.DispatchInfo.ConsumerType.FullName,
                    HandlerMethod = s.DispatchInfo.MessageHandlerMethod.Name,
                    DomainEventtype = s.DispatchInfo.MessageType.FullName
                }
            }).ToArray();
        }
    }
}
