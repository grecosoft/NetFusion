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
    internal class MessageBrokerModule : PluginModule
    {
        private bool _disposed;
        private IMessageBroker _messageBroker;
        private IEnumerable<MessageConsumer> _messageConsumers;

        // Discovered Properties:
        public IEnumerable<IMessageExchange> Exchanges { get; private set; }
        public IEnumerable<IMessageSerializerRegistry> Registries { get; private set; }
        
        protected override void Dispose(bool dispose)
        {
            if (dispose && !_disposed)
            {
                (_messageBroker as IDisposable)?.Dispose();
                _disposed = true;
            }

            base.Dispose(dispose);
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<MessageBroker>()
                .As<IMessageBroker>()
                .SingleInstance();
        }

        public override void Configure()
        {
            var messageModule = this.Context.GetPluginModule<IMessagingModule>();

            Type publisher = messageModule.MessagingConfig.PublisherTypes
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
            _messageBroker = scope.Resolve<IMessageBroker>();
            _messageConsumers = GetQueueConsumers(scope);

            BrokerSettings brokerSettings = GetBrokerSettings(scope);
            InitializeExchanges(brokerSettings);

            _messageBroker.Initialize(new MessageBrokerConfig
            {
                Settings = brokerSettings,
                Connections = GetConnections(brokerSettings),
                Serializers = GetMessageSerializers(),
                Exchanges = this.Exchanges
            });

            _messageBroker.DefineExchanges();
            _messageBroker.BindConsumers(_messageConsumers);
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

        private BrokerSettings GetBrokerSettings(ILifetimeScope container)
        {
            return container.Resolve<BrokerSettings>();
        }

        private void InitializeExchanges(BrokerSettings brokerSettings)
        {
            foreach(var exchange in this.Exchanges)
            {
                exchange.InitializeSettings();
                brokerSettings.ApplyQueueSettings(exchange);
            }
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

        private IDictionary<string, IMessageSerializer> GetMessageSerializers()
        {
            var serializers = GetConfiguredSerializers();
            AddSerializer(serializers, new JsonEventMessageSerializer());
            AddSerializer(serializers, new BinaryMessageSerializer());

            return serializers;
        }

        private IDictionary<string, IMessageSerializer> GetConfiguredSerializers()
        {
            IEnumerable<Type> allAppPluginTypes = this.Context.GetPluginTypesFrom(
                PluginTypes.AppHostPlugin, PluginTypes.AppComponentPlugin);

            IEnumerable<IMessageSerializerRegistry> registries = this.Registries.CreatedFrom(allAppPluginTypes);
            if (registries.Empty())
            {
                return new Dictionary<string, IMessageSerializer>();
            }

            if (registries.IsSingletonSet())
            {
                IEnumerable<IMessageSerializer> serializers = registries.First().GetSerializers();
                return serializers.ToDictionary(s => s.ContentType);
            }

            throw new ContainerException(
                $"The application host and its corresponding application plug-ins can only " +
                $"define one class implementing the: {typeof(IMessageSerializerRegistry)} interface-" +
                $"{registries.Count()} implementations were found.",

                registries.Select(e => new { Registry = e.GetType().AssemblyQualifiedName }));
        }

        private void AddSerializer(
            IDictionary<string, IMessageSerializer> serializers,
            IMessageSerializer serializer)
        {
            if (!serializers.Keys.Contains(serializer.ContentType))
            {
                serializers[serializer.ContentType] = serializer;
            }
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var brokerLog = new MessageBrokerLog(this.Exchanges, _messageConsumers);

            brokerLog.LogMessageExchanges(moduleLog);
            brokerLog.LogMessageConsumers(moduleLog);
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