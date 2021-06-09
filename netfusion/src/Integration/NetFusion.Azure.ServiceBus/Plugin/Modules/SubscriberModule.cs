using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Azure.ServiceBus.Plugin.Modules
{
    /// <summary>
    /// Module responsible for subscribing message handlers to Topic Subscriptions and Queues.
    /// </summary>
    public class SubscriberModule : PluginModule
    {
        // Dependent Modules:
        private INamespaceModule NamespaceModule { get; set; }
        private IMessageDispatchModule DispatchModule { get; set; }
        
        private EntitySubscription[] _entitySubscriptions;
        
        public override void Configure()
        {
            _entitySubscriptions = BuildEntitySubscriptions();
            
            ConfigureNamespaceSubscriptions(_entitySubscriptions);
        }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IQueueResponseService, QueueResponseService>();
        }

        // After building the list of entity subscriptions, invoke each subscription strategy
        // containing subscription logic specific to the type of entity be subscribed.
        protected override async Task OnStartModuleAsync(IServiceProvider services)
        {
            foreach (EntitySubscription subscription in _entitySubscriptions)
            {
                if (subscription.SubscriptionStrategy is IRequiresContext required)
                {
                    required.Context = GetContext(services, subscription);
                }
                
                NamespaceConnection conn = NamespaceModule.GetConnection(subscription.NamespaceName);
                await subscription.SubscriptionStrategy.Subscribe(conn);
            }
        }
        
        private NamespaceContext GetContext(IServiceProvider services, EntitySubscription entity) =>
            new(NamespaceModule, DispatchModule)
            {
                Logger = Context.LoggerFactory.CreateLogger(entity.SubscriptionStrategy.GetType().FullName),
                Serialization = services.GetRequiredService<ISerializationManager>()
            };

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            try
            {
                var tasks = _entitySubscriptions.Where(s => s.Processor != null)
                    .Select(s => s.Processor.CloseAsync());
                
                await Task.WhenAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                var flattenedEx = ex.Flatten();
                
                Context.Logger.LogError(flattenedEx, "Exception Disposing Service Bus Subscription.");
                throw flattenedEx;
            }
           
        }

        // ---------------------- Entity Subscriptions -----------------------
        
        private EntitySubscription[] BuildEntitySubscriptions()
        {
            var subscriptions = new List<EntitySubscription>();
            
            subscriptions.AddRange(GetQueueSubscriptions());
            subscriptions.AddRange(GetTopicSubscriptions());
            subscriptions.AddRange(GetRpcSubscriptions());

            return subscriptions.ToArray();
        }
        
        // Allow registries to apply any subscription specific settings for topics
        // defined within the namespace.
        private void ConfigureNamespaceSubscriptions(EntitySubscription[] subscriptions)
        {
            foreach (INamespaceRegistry registry in NamespaceModule.Registries)
            {
                var subscriptionsInNamespace = subscriptions
                    .Where(s => s.NamespaceName == registry.NamespaceName);

                registry.ConfigureSubscriptions(subscriptionsInNamespace);
                ApplySubscriptionSettings(subscriptions);
            }
        }

        // Overrides any settings specified within the service's configuration:
        private void ApplySubscriptionSettings(EntitySubscription[] subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                var config = NamespaceModule.GetSubscriptionConfig(subscription.NamespaceName, subscription.SettingsKey);
                if (config != null)
                {
                    subscription.ApplySettings(config);
                }
            }
        }
        
        // Returns a list of EntitySubscription objects for all message consumer handler methods
        // that are bound to a queue.
        private IEnumerable<QueueSubscription> GetQueueSubscriptions()
        {
            return GetDispatchersForSubscriptionType<QueueSubscriptionAttribute>()
                .Select(md =>
                {
                    var subAttrib = md.MessageHandlerMethod.GetAttribute<QueueSubscriptionAttribute>();
                    return new QueueSubscription(subAttrib.NamespaceName, subAttrib.EntityName)
                    {
                        DispatchInfo = md
                    };
                });
        }

        // Returns list of EntitySubscription objects for all message consumer handler methods
        // that are bound to a subscription on a specific topic
        private IEnumerable<TopicSubscription> GetTopicSubscriptions()
        {
            return GetDispatchersForSubscriptionType<TopicSubscriptionAttribute>()
                .Select(md =>
                {
                    var subAttrib = md.MessageHandlerMethod.GetAttribute<TopicSubscriptionAttribute>();
                    return new TopicSubscription(subAttrib.NamespaceName, subAttrib.EntityName, 
                        subAttrib.SubscriptionName, subAttrib.IsFanout)
                    {
                        DispatchInfo = md
                    };
                });
        }

        // To best utilize Service Bus resources, RPC based Command message are delivered over the same queue
        // for which the publisher awaits for a correlated response.  Multiple commands delivered to the same 
        // queue are identified by a Message Namespace value.  The message handler tagged with the same Message
        // Namespace value is invoked to process the command.  A service can declare multiple RPC based queues
        // to which sets of related commands are delivered.
        private IEnumerable<RpcQueueSubscription> GetRpcSubscriptions()
        {
            return GetDispatchersForSubscriptionType<RpcQueueSubscriptionAttribute>()
                .Select(md => new
                {
                    Attrib = md.MessageHandlerMethod.GetCustomAttribute<RpcQueueSubscriptionAttribute>(),
                    DispachInfo = md
                })
                .GroupBy(map => new {map.Attrib.NamespaceName, map.Attrib.EntityName})
                .Select(combined => new RpcQueueSubscription(
                    combined.Key.NamespaceName, 
                    combined.Key.EntityName, 
                    combined.Select(i => i.DispachInfo)));
        }

        // Returns list of all message consumer handler methods marked with a specific
        // type of Subscription attribute.
        private IEnumerable<MessageDispatchInfo> GetDispatchersForSubscriptionType<T>()
            where T : SubscriptionAttribute
        {
            return DispatchModule.AllMessageTypeDispatchers
                .Values()
                .Where(md => md.MessageHandlerMethod.HasAttribute<T>());
        }
        
        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["ServiceBusSubscriptions"] = _entitySubscriptions.Select(s => new
            {
                SubscriptionType = s.GetType().Name,
                s.NamespaceName,
                s.EntityName,
                MessageType = s.DispatchInfo?.MessageType.FullName,
                ConsumerType = s.DispatchInfo?.ConsumerType.FullName,
                HandlerName = s.DispatchInfo?.MessageHandlerMethod.Name
            });
        }
    }
}