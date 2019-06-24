using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types;
using NetFusion.Redis.Internal;
using NetFusion.Redis.Subscriber;
using NetFusion.Redis.Subscriber.Internal;
using StackExchange.Redis;

namespace NetFusion.Redis.Plugin.Modules
{
    /// <summary>
    /// Module that determines all domain-event handlers that should be subscribed to 
    /// a Redis channel.  When a message is received on the channel, the associated
    /// event-handler is invoked with the deserialized domain-event instance.
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        // Dependent Module Services:
        private IConnectionModule ConnModule { get; set; }
        private IMessageDispatchModule DispatchModule { get; set; }
        
        // Services:
        private ISerializationManager _serializationManager;
        
        // Message handlers subscribed to Redis channels:
        private MessageChannelSubscriber[] _subscribers;

        //------------------------------------------------------
        //--Plugin Initialization
        //------------------------------------------------------
        
        public override void Configure()
        {
            _subscribers = GetChannelSubscribers();
        }
        
        // Delegates to the core message dispatch module to find all message dispatch
        // handlers and filters the list to only those that should be bound to a channel.
        private MessageChannelSubscriber[] GetChannelSubscribers()
        {
            return DispatchModule.AllMessageTypeDispatchers
                .Values().Where(MessageChannelSubscriber.IsSubscriber)
                .Select(d => new MessageChannelSubscriber(d))
                .ToArray();
        }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ISubscriptionService, SubscriptionService>();
        }
        
        //------------------------------------------------------
        //--Plugin Execution
        //------------------------------------------------------

        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            _serializationManager = services.GetRequiredService<ISerializationManager>();

            return SubscribeToChannels(_subscribers);
        }
        
        private async Task SubscribeToChannels(IEnumerable<MessageChannelSubscriber> subscribers)
        {
            foreach (var msgSubscriber in subscribers)
            {
                ISubscriber subscriber = ConnModule.GetSubscriber(msgSubscriber.DatabaseName);
                
                // Callback invoked when message published to channel:
                await subscriber.SubscribeAsync(msgSubscriber.Channel, (channel, message) =>
                {
                    Type messageType = msgSubscriber.DispatchInfo.MessageType;
                    var messageParts = ChannelMessageEncoder.UnPack(message);
                    
                    // Deserialize message byte array into domain-event type associated with handler:
                    IDomainEvent domainEvent = (IDomainEvent)_serializationManager.Deserialize(
                        messageParts.contentType, 
                        messageType, 
                        messageParts.messageData);

                    LogReceivedDomainEvent(channel, domainEvent, msgSubscriber);
                    
                    // Invoke the in-process handler:
                    DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
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

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Channel:Subscribers"] = _subscribers.Select(s => new
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
