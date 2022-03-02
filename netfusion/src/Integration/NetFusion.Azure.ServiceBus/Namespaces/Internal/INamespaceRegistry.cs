using System.Collections.Generic;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Base.Plugins;

namespace NetFusion.Azure.ServiceBus.Namespaces.Internal
{
    /// <summary>
    /// Implementations determine the entities created within a specific namespace and the 
    /// message types that should be delivered. Service Bus entities refer to Queues and Topics.
    /// </summary>
    public interface INamespaceRegistry : IKnownPluginType
    {
        /// <summary>
        /// The namespace in which the entities should be created.
        /// </summary>
        public string NamespaceName { get; }
        
        /// <summary>
        /// The entities to be created within the namespace.
        /// </summary>
        /// <returns>List of metadata for the entities to be created.</returns>
        public IEnumerable<NamespaceEntity> GetNamespaceEntities();
        
        /// <summary>
        /// Called to allow the registry to apply any additional subscription
        /// settings.
        /// </summary>
        /// <param name="subscriptions">List of created subscriptions.</param>
        void ConfigureSubscriptions(IEnumerable<EntitySubscription> subscriptions);
    }
}