using System.Collections.Generic;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Base.Plugins;

namespace NetFusion.Azure.ServiceBus.Namespaces.Internal
{
    /// <summary>
    /// Implementations determine the entities that should be created with a specific namespace.
    /// </summary>
    public interface INamespaceRegistry : IKnownPluginType
    {
        /// <summary>
        /// The namespace in which the entities should be created.
        /// </summary>
        public string NamespaceName { get; }
        
        /// <summary>
        /// The namespace entities to be created within the namespace.
        /// </summary>
        /// <returns>List of metadata for the entities to be created.</returns>
        public IEnumerable<NamespaceEntity> GetNamespaceEntities();
        
        /// <summary>
        /// Called to allow the registry to apply any additional settings
        /// to the created subscriptions.
        /// </summary>
        /// <param name="subscriptions">List of subscriptions created.</param>
        void ConfigureSubscriptions(IEnumerable<EntitySubscription> subscriptions);
    }
}