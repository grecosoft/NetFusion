using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Azure.ServiceBus.Plugin.Modules
{
    /// <summary>
    /// Module responsible for creating and configuring Azure Service Bus topics
    /// and queues configured within the code by INamespaceRegistry instances.
    /// For each entity, a corresponding message sender is created and used for
    /// the life of the Microservice.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {
        // Dependent Modules:
        public INamespaceModule NamespaceModule { get; private set; }
        public IMessageDispatchModule DispatchModule { get; private set; }

        private NamespaceEntity[] _entities;
        
        // Map between message type and service-bus entity:
        private Dictionary<Type, NamespaceEntity> _primaryEntities;

        public override void Initialize()
        {
            _entities = NamespaceModule.Registries
                .SelectMany(r => r.GetNamespaceEntities())
                .ToArray();

            _primaryEntities = _entities.Where(ne => !ne.IsSecondaryQueue)
                .ToDictionary(ne => ne.MessageType);

            ValidateEntityDependencies();
        }

        // Validate that for each entity specifying a Forwarding queue that the
        // corresponding entity exists.
        private void ValidateEntityDependencies()
        {
            var queues = _entities.OfType<QueueMeta>();

            var forwardedQueues = queues.Select(q => new[] { q.ForwardDeadLetteredMessagesTo, q.ForwardTo })
                .SelectMany(v => v)
                .Where(fqn => fqn != null)
                .Distinct();

            var missingQueues = forwardedQueues.Where(forwardQueueName =>
                _entities.Count(dq => dq.EntityName == forwardQueueName) == 0)
                .ToArray();

            if (missingQueues.Any())
            {
                throw new InvalidOperationException(
                    $"The following secondary queues have not been defined: {string.Join(", ", missingQueues)}.");
            }
        }

        protected override async Task OnStartModuleAsync(IServiceProvider services)
        {       
            await CreateSecondaryEntities();
            await CreatePrimaryEntities(services);
        }

        private async Task CreatePrimaryEntities(IServiceProvider services)
        {
            foreach (NamespaceEntity entity in _primaryEntities.Values)
            {
                if (entity.EntityStrategy is IRequiresContext required)
                {
                    required.Context = GetContext(services, entity);
                }
                
                NamespaceConnection conn = NamespaceModule.GetConnection(entity.NamespaceName);

                await entity.EntityStrategy.CreateEntityAsync(conn);
                entity.EntitySender = conn.BusClient.CreateSender(entity.EntityName);
            }
        }

        private async Task CreateSecondaryEntities()
        {
            foreach (NamespaceEntity entity in _entities.Where(ne => ne.IsSecondaryQueue))
            {
                NamespaceConnection conn = NamespaceModule.GetConnection(entity.NamespaceName);
                await entity.EntityStrategy.CreateEntityAsync(conn);
            }
        }
        
        private NamespaceContext GetContext(IServiceProvider services, NamespaceEntity entity) =>
            new NamespaceContext(NamespaceModule, DispatchModule)
            {
                Logger = Context.LoggerFactory.CreateLogger(entity.EntityStrategy.GetType().FullName),
                Serialization = services.GetRequiredService<ISerializationManager>()
            };
    

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach (NamespaceEntity entity in _primaryEntities.Values)
            {
                if (entity.EntitySender != null)
                {
                    await entity.EntitySender.CloseAsync();
                }
                
                // Determine if the entity strategy supports custom cleanup logic:
                if (entity.EntityStrategy is ICleanupStrategy supported)
                {
                    NamespaceConnection conn = NamespaceModule.GetConnection(entity.NamespaceName);
                    await supported.CleanupEntityAsync(conn);
                }
            }
        }

        public bool TryGetMessageEntity(Type messageType, out NamespaceEntity entity)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            
            return (entity = _primaryEntities.ContainsKey(messageType) ? _primaryEntities[messageType] : null) != null;
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["ServiceBusEntities"] = _primaryEntities.Values.Select(e => new
            {
                MessageType = e.MessageType.FullName,
                e.NamespaceName,
                e.EntityName,
                e.ContentType,
                e.IsSecondaryQueue
            }).ToArray();
        }
    }
}