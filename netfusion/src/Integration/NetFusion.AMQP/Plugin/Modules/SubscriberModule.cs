﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Amqp;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.AMQP.Subscriber;
using NetFusion.AMQP.Subscriber.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Plugin;

namespace NetFusion.AMQP.Plugin.Modules
{
    /// <summary>
    /// Plugin module when bootstrapped discovers the message handlers corresponding to
    /// host defined items (i.e. Queues/Topics) to which they should be subscribed.
    /// </summary>
    public class SubscriberModule : PluginModule
    {       
        private static readonly object ReceiverReConnLock = new object();
        
        // Dependent Modules:
        private IConnectionModule _connectionModule;
        private IMessageDispatchModule _dispatchModule;
        private ISerializationManager _serialization;
        
        // Host provided service to alter module's default behavior.  For example, the host
        // can create an unique topic subscription to which it can subscribe - the creation
        // of topic subscriptions is not part the the AMQP specification.
        private ISubscriptionSettings _subscriptionSettings;
        
        // Message handlers subscribed to host items such as queues and topics.
        private HostItemSubscriber[] _subscribers;

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ISubscriptionSettings, NullSubscriptionSettings>();
        }
        
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            _connectionModule = services.GetRequiredService<IConnectionModule>();
            _dispatchModule = services.GetRequiredService<IMessageDispatchModule>();
            _serialization = services.GetRequiredService<ISerializationManager>();
            
            _subscriptionSettings = services.GetRequiredService<ISubscriptionSettings>();
            _subscribers = GetHostSubscribers(_dispatchModule);

            return LinkHandlersToHostItems(_subscriptionSettings);
        }

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            
            foreach(var itemSubscriber in _subscribers)
            {
                if (itemSubscriber.ReceiverLink != null)
                {
                    await itemSubscriber.ReceiverLink.CloseAsync();
                }
            }

            if (_subscriptionSettings != null)
            {
                await _subscriptionSettings.CleanupSettings();
            }
        }

        private static HostItemSubscriber[] GetHostSubscribers(IMessageDispatchModule dispatchModule)
        {
            // Finds all message handler methods that subscribe to AMQP Queues and Topics.
            return dispatchModule.AllMessageTypeDispatchers
                .Values().Where(HostItemSubscriber.IsSubscriber)
                .Select(d => new HostItemSubscriber(d))
                .ToArray();
        }

        private async Task LinkHandlersToHostItems(ISubscriptionSettings subscriptionSettings)
        {
            if (subscriptionSettings == null) throw new ArgumentNullException(nameof(subscriptionSettings));
         
            // Initialize any host provided AMQP settings - i.e. creating host specific
            // subscriptions to AMQP defined topics.
            await _subscriptionSettings.ConfigureSettings();
            
            _connectionModule.SetReceiverConnectionCloseHandler(ReSetReceiverLinkOnClosedConn);

            foreach (var subscriber in _subscribers)
            {
                LinkSubscriber(subscriber, subscriptionSettings);
            }
        }

        // Obtain the session on which the AMQP receiver link should be created and then delegate
        // to the ISubscriberLinker implementation associated with the host item (Queue/Topic).
        // The subscriber-linker will bind the message handler to the host item so it will be
        // invoked when a message is received.
        private void LinkSubscriber(HostItemSubscriber subscriber, ISubscriptionSettings subscriptionSettings)
        {
            Session session = _connectionModule.CreateReceiverSession(subscriber.HostAttribute.HostName);
            ISubscriberLinker linker = subscriber.HostItemAttribute.Linker;
            
            linker.SetServices(_dispatchModule, _serialization, Context.LoggerFactory);
            
            // Delegate to the subscriber linker implementation to handle received messages:
            linker.LinkSubscriber(session, subscriber, subscriptionSettings);
        }

        private void ReSetReceiverLinkOnClosedConn(string closedHostName)
        {
            lock (ReceiverReConnLock)
            {
                var closedSubscribers = _subscribers.Where(s => s.HostAttribute.HostName == closedHostName);

                foreach (HostItemSubscriber subscriber in closedSubscribers)
                {
                    LinkSubscriber(subscriber, _subscriptionSettings);
                }
            }
        }
    }
}