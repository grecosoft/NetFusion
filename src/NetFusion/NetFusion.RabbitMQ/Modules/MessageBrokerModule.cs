﻿using Autofac;
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

namespace NetFusion.RabbitMQ.Modules
{
    /// <summary>
    /// Plug-in module that discovers all defined RabbitMQ exchanges and consumer bindings.
    /// This plug-in delegates to the messaging plug-in allowing for the publishing of
    /// messages to exchanges.  The messaging plug-in is also delegated to when a message
    /// is received from a queue and needs to be dispatched to is consumers.  
    /// </summary>
    internal class MessageBrokerModule : PluginModule
    {
        private bool _disposed;
        private BrokerSettings _brokerSettings;
        private IMessageBroker _messageBroker;
        private IEnumerable<MessageConsumer> _messageConsumers;

        // Discovered Properties:
        private IEnumerable<IMessageExchange> Exchanges { get; set; }
        private IEnumerable<IMessageSerializerRegistry> Registries { get; set; }
        
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
                this.Context.Logger.Warning(
                    $"Exchanges have been declared but the publisher of type: {typeof(RabbitMqMessagePublisher)} " + 
                    $"has not been registered-for messages to be published, this class must be registered." );
            }
        }

        // Creates the message broker that initializes the exchanges and
        // determines the queues that should be created.
        public override void StartModule(ILifetimeScope scope)
        {
            _messageBroker = scope.Resolve<IMessageBroker>();
            _brokerSettings = GetBrokerSettings(scope);
            _messageConsumers = GetQueueConsumers(scope);
            
            InitializeExchanges(_brokerSettings);

            _messageBroker.Initialize(new MessageBrokerConfig
            {
                Settings = _brokerSettings,
                Connections = GetConnections(_brokerSettings),
                Serializers = GetMessageSerializers(),
                Exchanges = this.Exchanges
            });

            _messageBroker.DefineExchanges();
            _messageBroker.BindConsumers(_messageConsumers);

            SaveExchangeMetadata(scope);
        }

        private BrokerSettings GetBrokerSettings(ILifetimeScope container)
        {
            return container.Resolve<BrokerSettings>();
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

        private void InitializeExchanges(BrokerSettings brokerSettings)
        {
            foreach(IMessageExchange exchange in this.Exchanges)
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

            IEnumerable<string> duplicateBrokerNames = settings.Connections.GroupBy(c => c.BrokerName)
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

            // Add the default serializers if not overridden by the application host.
            AddSerializer(serializers, new JsonEventMessageSerializer());
            AddSerializer(serializers, new BinaryMessageSerializer());

            return serializers;
        }

        private IDictionary<string, IMessageSerializer> GetConfiguredSerializers()
        {
            // Find all serializer registries which specify any custom serializers or
            // overrides for a given content type.  Only look in application specific
            // plug-ins.
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

        public override void StopModule(ILifetimeScope scope)
        {
            if (_brokerSettings.Connections != null)
            {
                foreach (BrokerConnection broker in _brokerSettings.Connections)
                {
                     broker.Connection?.Close();
                }
            }
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var brokerLog = new MessageBrokerLog(this.Exchanges, _messageConsumers);

            brokerLog.LogMessageExchanges(moduleLog);
            brokerLog.LogMessageConsumers(moduleLog);
        }

        // Save the exchanges and the associated queues.  This will be needed when running
        // in a cluster when a RabbitMQ node goes down.  The producer/consumer recovering 
        // from the exception will needs to create the exchanges and queues on the new node.
        private void SaveExchangeMetadata(ILifetimeScope scope)
        {
            var exchangeRep = scope.Resolve<IBrokerMetaRepository>();
            IEnumerable<BrokerMeta> brokerMeta = GetExchangeConfig();

            exchangeRep.SaveAsync(brokerMeta).Wait();
        }

        // Only save the queues that are not exclusive or marked for auto deletion.  
        // Since these queues will be removed when connected consumers disconnect,
        // the queue contains messages for which black-holed ones are desirable
        // (they will be re-created when the consumer's broker reconnects).
        private IEnumerable<BrokerMeta> GetExchangeConfig()
        {
            var publisherExchanges = this.GetPublisherMetadata();
            var consumerExchanges = this.GetConsumerMetadata();

            return publisherExchanges.Concat(consumerExchanges)
                .GroupBy(e => e.BrokerName)
                .Select(g => new BrokerMeta
                {
                    ApplicationId = Context.AppHost.Manifest.PluginId,
                    BrokerName = g.Key,
                    ExchangeMeta = g.ToList()
                });
        }

        private IEnumerable<ExchangeMeta> GetPublisherMetadata()
        {
            return this.Exchanges.Select(e => new ExchangeMeta
            {
                BrokerName = e.BrokerName,
                ExchangeName = e.ExchangeName,
                Settings = e.Settings,

                QueueMeta = e.Queues.Select(q => new QueueMeta
                {
                    QueueName = q.QueueName,
                    RouteKeys = q.RouteKeys,
                    Settings = q.Settings
                }).ToList()
            });
        }

        private IEnumerable<ExchangeMeta> GetConsumerMetadata()
        {
            var namedConsumerQueues = _messageConsumers.Where(IsConsumerConfiguredQueue);

            return namedConsumerQueues.Select(c => new ExchangeMeta
            {
                BrokerName = c.BrokerName,
                ExchangeName = c.ExchangeName,
                QueueMeta = new[] { new QueueMeta {
                    QueueName = c.QueueName,
                    RouteKeys = c.RouteKeys,
                    Settings = c.QueueSettings
                }}
            });
        }

        private bool IsConsumerConfiguredQueue(MessageConsumer consumer)
        {
            return !consumer.QueueSettings.IsBrokerAssignedName 
                && !consumer.QueueSettings.IsAutoDelete 
                && !consumer.QueueSettings.IsExclusive
                && consumer.BindingType == QueueBindingTypes.Create;
        }
    }
}