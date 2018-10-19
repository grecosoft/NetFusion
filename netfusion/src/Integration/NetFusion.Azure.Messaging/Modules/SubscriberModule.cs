using System;
using System.Linq;
using System.Threading.Tasks;
using Amqp;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Azure.Messaging.Subscriber;
using NetFusion.Azure.Messaging.Subscriber.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Modules;

namespace NetFusion.Azure.Messaging.Modules
{
    /// <summary>
    /// Plugin module when bootstrapped discovers the message handlers
    /// corresponding to Azure namespace items (i.e. Queues/Topics) to
    /// which they should be subscribed.
    /// </summary>
    public class SubscriberModule : PluginModule,
        ISubscriberModule
    {
        private bool _disposed;
        
        // Dependent Modules:
        private IConnectionModule _connectionModule;
        private IMessageDispatchModule _dispatchModule;
        private ISerializationManager _serialization;
        
        // Message handlers subscribed to Azure namespace objects.
        private NamespaceItemSubscriber[] _subscribers;
        
        public override void StartModule(IServiceProvider services)
        {
            _connectionModule = services.GetRequiredService<IConnectionModule>();
            _dispatchModule = services.GetRequiredService<IMessageDispatchModule>();
            _serialization = services.GetRequiredService<ISerializationManager>();

            _subscribers = GetNamespaceSubscribers(_dispatchModule);
        }

        private static NamespaceItemSubscriber[] GetNamespaceSubscribers(IMessageDispatchModule dispatchModule)
        {
            return dispatchModule.AllMessageTypeDispatchers
                .Values().Where(NamespaceItemSubscriber.IsSubscriber)
                .Select(d => new NamespaceItemSubscriber(d))
                .ToArray();
        }

        // Called by an application component such as an Hosted Service.
        public async Task LinkHandlersToNamespaces()
        {
            foreach (var subscriber in _subscribers)
            {
                await LinkSubscriber(subscriber);
            }
        }

        // Obtain the session on which the AMQP receiver link should be created and then delegate
        // to the ISubscriberLinker implementation associated with the namespace item (Queue/Topic).
        // The subscriber-linker will bind the message handler to the namespace item so it will be
        // invoked when a message is received.
        private async Task LinkSubscriber(NamespaceItemSubscriber subscriber)
        {
            Session nsSession = await _connectionModule.GetSession(subscriber.NamespaceAttrib.NamespaceName);
            ISubscriberLinker linker = subscriber.NamespaceItemAttrib.Linker;
            
            // Set the services used by the linker when messages are received:
            linker.DispatchModule = _dispatchModule;
            linker.Serialization = _serialization;
            linker.LoggerFactory = Context.LoggerFactory;
            
            // Delegate to the subscriber linker implementation to handle received messages:
            subscriber.NamespaceItemAttrib.Linker.LinkSubscriber(nsSession, subscriber);
        }
        
        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(var nsItemSubscriber in _subscribers)
            {
                nsItemSubscriber.ReceiverLink?.Close();
            }

            _disposed = true;
        }
    }
}