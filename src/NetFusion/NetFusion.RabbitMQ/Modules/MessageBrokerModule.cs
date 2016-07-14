using Autofac;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Exchanges;
using NetFusion.RabbitMQ.Integration;
using NetFusion.RabbitMQ.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plug-in module that discovers all defined RabbitMQ exchanges and consumer bindings.
    /// This plug-in builds on top of the messaging pattern allowing for the publishing
    /// of messages to exchanges.  Consumers are bound to Queues by marking message 
    /// handlers with a Queue Consumer derived attribute.  
    /// </summary>
    internal class MessageBrokerModule : PluginModule,
        IMessageBrokerModule
    {
        private MessageBroker _messageBroker;
        private bool _disposed;

        public IMessageBroker MessageBroker { get { return _messageBroker; } }

        public IEnumerable<IMessageExchange> Exchanges { get; private set; }
        public IEnumerable<IMessageSerializerRegistry> Registries { get; private set; }

        private IEnumerable<MessageConsumer> _messageConsumers;

        protected override void Dispose(bool dispose)
        {
            if (dispose && !_disposed)
            {
                _messageBroker.Dispose();
                _disposed = true;
            }

            base.Dispose(dispose);
        }

        public override void Configure()
        {
            var messageModule = this.Context.GetPluginModule<IMessagingModule>();

            var publisher = messageModule.MessagingConfig.PublisherTypes
                .FirstOrDefault(pt => pt == typeof(RabbitMqMessagePublisher));

            if (publisher == null && this.Exchanges.Any())
            {
                Plugin.Log.Warning(
                    $"exchanges have been declared but the publisher of type: {typeof(RabbitMqMessagePublisher)} " + 
                    $"has not been registered-for messages to be published, this class must be registered." );
            }
        }

        // Creates the message broker that initializes the exchanges and
        // determines the queues that should be created.
        public override void StartModule(ILifetimeScope scope)
        {
            var brokerSettings = GetBrokerSettings(scope);
            var connections = GetConnections(brokerSettings);

            _messageConsumers = GetQueueConsumers(scope);

            _messageBroker = new MessageBroker(brokerSettings, connections, this.Exchanges);
            _messageBroker.DefineExchanges();
            _messageBroker.BindConsumers(_messageConsumers);
            _messageBroker.SetExchangeMetadataReader(host => ReadExchangeMetadataAsync(host, scope));

            // Add any discovered message encoders from the host.
            var serializers = GetCustomSerializers();
            serializers.ForEach(_messageBroker.AddSerializer);

            // Exchange meta-data will be read when recovering from a failed RabbitMQ node.
            // SaveExchangeMetadata(container);
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var brokerLog = new MessageBrokerLog(this.Exchanges, _messageConsumers);

            brokerLog.LogMessageExchanges(moduleLog);
            brokerLog.LogMessageConsumers(moduleLog);
        }

        private BrokerSettings GetBrokerSettings(ILifetimeScope container)
        {
            return container.Resolve<BrokerSettings>();   
        }

        private IDictionary<string, BrokerConnection> GetConnections(BrokerSettings settings)
        {
            if (settings.Connections == null)
            {
                return new Dictionary<string, BrokerConnection>();
            }

            var duplicateBrokerNames = settings.Connections.GroupBy(c => c.BrokerName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateBrokerNames.Any())
            {
                throw new InvalidOperationException(
                    $"The following broker names are specified more than " + 
                    $"once: {String.Join(", ", duplicateBrokerNames)}.");
            }

            return settings.Connections.ToDictionary(c => c.BrokerName);
        }

        // Finds all consumers message handler methods that are associated with a queue 
        // and associates the queue meta-data with the message dispatch meta-data.
        private IEnumerable<MessageConsumer> GetQueueConsumers(ILifetimeScope container)
        {
            var messagingServices = container.Resolve<IMessagingModule>();

            // All message handlers that are consumers of a queue that should be
            // called when an event is received based on the exchange distribution rules.
            return messagingServices.AllMessageTypeDispatchers
                .Values().Where(IsQueueConsumer)
                .Select(d => new
                {
                    ConsumerBroker = d.ConsumerType.GetCustomAttribute<BrokerAttribute>(),
                    ConsumerQueue = d.MessageHandlerMethod.GetCustomAttribute<QueueConsumerAttribute>(),
                    DispatchInfo = d,
                })
                .Select(d => new MessageConsumer(d.ConsumerBroker, d.ConsumerQueue, d.DispatchInfo))
                .ToList();
        }

        // The criteria that determines if a given consumer event handler method
        // is bound to a queue.
        private bool IsQueueConsumer(MessageDispatchInfo dispatchInfo)
        {
            return dispatchInfo.ConsumerType.HasAttribute<BrokerAttribute>()
                && dispatchInfo.MessageHandlerMethod.HasAttribute<QueueConsumerAttribute>();
        }

        private IEnumerable<IMessageSerializer> GetCustomSerializers()
        {
            var allAppPluginTypes = this.Context.GetPluginTypesFrom(
                PluginTypes.AppHostPlugin, PluginTypes.AppComponentPlugin);

            var registries = this.Registries.CreatedFrom(allAppPluginTypes);
            if (registries.Empty())
            {
                return Enumerable.Empty<IMessageSerializer>();
            }

            if (registries.IsSingletonSet())
            {
                return registries.First().GetSerializers();
            }

            throw new ContainerException(
                $"The application host and its corresponding application plug-ins can only " + 
                $"define one class implementing the: {typeof(IMessageSerializerRegistry)} interface-" +  
                $"{registries.Count()} implementations were found.",

                registries.Select(e => new { Registry = e.GetType().AssemblyQualifiedName }));
        }

        // TODO:  Review the following:

        // Save the exchanges and the associated queues.  This will be needed
        // when running in a cluster when a RabbitMQ node goes down.  The
        // producer/consumer recovering from the exception will need to create
        // the exchanges and queues on the new node.
        private void SaveExchangeMetadata(IContainer container)
        {
            if (this.Exchanges.Empty()) return;

            using (var scope = container.BeginLifetimeScope())
            {
                var repository = container.Resolve<IExchangeRepository>();
                var exchanges = GetExchangeMetadata();
                repository.Save(exchanges);
            }
        }

        private IEnumerable<ExchangeConfig> GetExchangeMetadata()
        {
            return this.Exchanges.Select(e => new ExchangeConfig
            {
                BrokerName = e.BrokerName,
                ExchangeName = e.ExchangeName,
                Settings = e.Settings,

                QueueConfigs = e.Queues.Select(q => new QueueConfig
                {
                    QueueName = q.QueueName,
                    RouteKeys = q.RouteKeys,
                    Settings = q.Settings
                }).ToList()
            });
        }

        private async static Task<IEnumerable<ExchangeConfig>> ReadExchangeMetadataAsync(string hostName, ILifetimeScope container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var repository = container.Resolve<IExchangeRepository>();
                return await repository.LoadAsync(hostName);
            }
        }
    }
}