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
        private INamespaceModule NamespaceModule { get; set; }
        private IMessageDispatchModule DispatchModule { get; set; }

        private NamespaceEntity[] _entities;
        
        // Map between message type and service-bus entity to which it is published.
        // Primary entities are those to which the Microservice publishes messages.
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

        // Validate each entity specifying a Forwarding secondary queue has been defined,
        private void ValidateEntityDependencies()
        {
            var queues = _entities.OfType<QueueMeta>();

            var forwardedQueues = queues.Select(q => new[] { q.ForwardDeadLetteredMessagesTo, q.ForwardTo })
                .SelectMany(v => v)
                .Where(fqn => fqn != null)
                .Distinct();

            var missingQueues = forwardedQueues.Where(fqn =>
                _entities.Any(dq => dq.EntityName == fqn) == false)
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
        
        // Secondary queues are those to which Azure Service Bus automatically delivers messages.
        // The Microservice does not directly publish messages to these queues.
        private async Task CreateSecondaryEntities()
        {
            foreach (NamespaceEntity entity in _entities.Where(ne => ne.IsSecondaryQueue))
            {
                NamespaceConnection conn = NamespaceModule.GetConnection(entity.NamespaceName);
                await entity.EntityStrategy.CreateEntityAsync(conn);
            }
        }

        // Primary entities are those to which the Microservice publishes messages.
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
                
                // Since this is a primary entity to which messages will be sent,
                // create a corresponding Service Bus sender.
                entity.EntitySender = conn.BusClient.CreateSender(entity.EntityName);
            }
        }
        
        private NamespaceContext GetContext(IServiceProvider services, NamespaceEntity entity) =>
            new(NamespaceModule, DispatchModule)
            {
                Logger = Context.LoggerFactory.CreateLogger(entity.EntityStrategy.GetType().FullName),
                Serialization = services.GetRequiredService<ISerializationManager>()
            };
    

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach (NamespaceEntity entity in _primaryEntities.Values
                .Where(pe => pe.EntitySender != null))
            {
                await entity.EntitySender.CloseAsync();

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