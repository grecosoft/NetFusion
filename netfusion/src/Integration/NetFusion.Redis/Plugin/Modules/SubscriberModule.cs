using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types.Contracts;
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
        private ILogger<SubscriberModule> _logger;
        
        // Dependent Module Services:
        protected IConnectionModule ConnModule { get; set; }
        protected IMessageDispatchModule DispatchModule { get; set; }
        
        // Services:
        private ISerializationManager _serializationManager;
        private IMessageLogger _messageLogger;
        
        // Message handlers subscribed to Redis channels:
        private MessageChannelSubscriber[] _subscribers;

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

        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            _logger = services.GetService<ILogger<SubscriberModule>>();
            _serializationManager = services.GetRequiredService<ISerializationManager>();
            _messageLogger = services.GetRequiredService<IMessageLogger>();

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
                    var (contentType, messageData) = ChannelMessageEncoder.UnPack(message);
                    
                    // Deserialize message byte array into domain-event type associated with handler:
                    IDomainEvent domainEvent = (IDomainEvent)_serializationManager.Deserialize(
                        contentType, 
                        messageType, 
                        messageData);

                    var msgLog = new MessageLog(domainEvent, LogContextType.ReceivedMessage);
                    
                    LogReceivedDomainEvent(channel, domainEvent, msgSubscriber);
                    AddMessageLogDetails(msgLog, channel, msgSubscriber);

                    try
                    {
                        // Invoke the in-process handler:
                        DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
                            msgSubscriber.DispatchInfo, 
                            domainEvent).Wait();
                    }
                    catch (Exception ex)
                    {
                        msgLog.AddLogError("Channel Subscription", ex);
                        throw;
                    }
                    finally { _messageLogger.LogAsync(msgLog).Wait(); }
                });
            }
        }
        
        // ------------------------------ [Logging] -------------------------------

        private void LogReceivedDomainEvent(string channel, IDomainEvent domainEvent,
            MessageChannelSubscriber subscriber)
        {
            var channelInfo = new
            {
                Channel = channel,
                Subscription = subscriber.Channel,
            };

            var log = LogMessage.For(LogLevel.Information,
                "Domain event {EventName} received on Redis Channel",
                domainEvent.GetType().Name).WithProperties(
                    new LogProperty { Name = "ChannelInfo", Value = channelInfo, DestructureObjects = true }, 
                    new LogProperty { Name = "DomainEvent", Value = domainEvent, DestructureObjects = true });
            
            _logger.Write(log);
        }

        private void AddMessageLogDetails(MessageLog msgLog, string channel, MessageChannelSubscriber subscriber)
        {
            if (! _messageLogger.IsLoggingEnabled) return;
            
            msgLog.SentHint("subscribe-redis");
            msgLog.AddLogDetail("Channel", channel);
            msgLog.AddLogDetail("Subscription", subscriber.Channel);
            msgLog.AddLogDetail("Handler Type", subscriber.DispatchInfo.ConsumerType.FullName);
            msgLog.AddLogDetail("Handler Method", subscriber.DispatchInfo.MessageHandlerMethod.Name);
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["ChannelSubscribers"] = _subscribers.Select(s => new
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
