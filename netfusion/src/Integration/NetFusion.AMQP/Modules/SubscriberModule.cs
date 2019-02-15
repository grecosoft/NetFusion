using System;
using System.Linq;
using System.Threading.Tasks;
using Amqp;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Modules;
using NetFusion.AMQP.Subscriber;
using NetFusion.AMQP.Subscriber.Internal;

namespace NetFusion.AMQP.Modules
{
    /// <summary>
    /// Plugin module when bootstrapped discovers the message handlers
    /// corresponding to host defined items (i.e. Queues/Topics) to
    /// which they should be subscribed.
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        private bool _disposed;
        
        // Dependent Modules:
        private IConnectionModule _connectionModule;
        private IMessageDispatchModule _dispatchModule;
        private ISerializationManager _serialization;
        
        // Host provided service to alter module's default behavior.
        private ISubscriptionSettings _subscriptionSettings;
        
        // Message handlers subscribed to host items such as queues and topics.
        private HostItemSubscriber[] _subscribers;

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<ISubscriptionSettings, NullSubscriptionSettings>();
        }
        
        public override void StartModule(IServiceProvider services)
        {
            _connectionModule = services.GetRequiredService<IConnectionModule>();
            _dispatchModule = services.GetRequiredService<IMessageDispatchModule>();
            _serialization = services.GetRequiredService<ISerializationManager>();
            
            _subscriptionSettings = services.GetRequiredService<ISubscriptionSettings>();
            _subscribers = GetHostSubscribers(_dispatchModule);
            
            LinkHandlersToHostItems(_subscriptionSettings).Wait();
        }

        private static HostItemSubscriber[] GetHostSubscribers(IMessageDispatchModule dispatchModule)
        {
            return dispatchModule.AllMessageTypeDispatchers
                .Values().Where(HostItemSubscriber.IsSubscriber)
                .Select(d => new HostItemSubscriber(d))
                .ToArray();
        }

        // Called by an service component such as an .NET Core Hosted Service.
        private async Task LinkHandlersToHostItems(ISubscriptionSettings subscriptionSettings)
        {
            if (subscriptionSettings == null) throw new ArgumentNullException(nameof(subscriptionSettings));
         
            await _subscriptionSettings.ConfigureSettings();
            
            foreach (var subscriber in _subscribers)
            {
                await LinkSubscriber(subscriber, subscriptionSettings);
            }
        }

        // Obtain the session on which the AMQP receiver link should be created and then delegate
        // to the ISubscriberLinker implementation associated with the host item (Queue/Topic).
        // The subscriber-linker will bind the message handler to the host item so it will be
        // invoked when a message is received.
        private async Task LinkSubscriber(HostItemSubscriber subscriber, ISubscriptionSettings subscriptionSettings)
        {
            Session session = await _connectionModule.GetSession(subscriber.HostAttribute.HostName);
            ISubscriberLinker linker = subscriber.HostItemAttribute.Linker;
            
            // Set the services used by the linker when messages are received:
            linker.DispatchModule = _dispatchModule;
            linker.Serialization = _serialization;
            linker.LoggerFactory = Context.LoggerFactory;
            
            // Delegate to the subscriber linker implementation to handle received messages:
            linker.LinkSubscriber(session, subscriber, subscriptionSettings);
        }
        
        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(var itemSubscriber in _subscribers)
            {
                itemSubscriber.ReceiverLink?.Close();
            }

            _subscriptionSettings?.CleanupSettings();

            _disposed = true;
        }
    }
}