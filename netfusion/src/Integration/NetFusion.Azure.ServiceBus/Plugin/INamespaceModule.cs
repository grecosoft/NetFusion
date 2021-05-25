using System.Collections.Generic;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Plugin.Configs;
using NetFusion.Azure.ServiceBus.Settings;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Azure.ServiceBus.Plugin
{
    /// <summary>
    /// Module service for managing connections to Azure Service Bus Namespaces
    /// and provides access to registries specifying how namespace entities are
    /// mapped to commands and domain-events.
    /// </summary>
    public interface INamespaceModule : IPluginModuleService
    {
        /// <summary>
        /// Overall Service Bus plug-in configuration settings specified when
        /// the plugin is bootstrapped by the host.
        /// </summary>
        ServiceBusConfig BusConfig { get; }
        
        /// <summary>
        /// Reference to defined  namespace registries specifying how Service Bus
        /// entities are configured and mapped to command and domain-events.
        /// </summary>
        IEnumerable<INamespaceRegistry> Registries { get; }
        
        /// <summary>
        /// Returns a class containing both the messaging and administration clients used
        /// to issue commands to Azure Service Bus.
        /// </summary>
        /// <param name="namespaceName">The namespace for the configured connection.</param>
        /// <returns>The configured connection or an exception if no connection exists for
        /// the provided namespace.</returns>
        NamespaceConnection GetConnection(string namespaceName);

        /// <summary>
        /// Returns settings for a subscription defined within the application's configuration.
        /// </summary>
        /// <param name="namespaceName">The namespace in which the subscription is defined.</param>
        /// <param name="settingsKey">Value identifying the subscription.  For a subscription to a queue,
        /// the queue's name is used.  If a subscription to a topic, the topic and subscription names are
        /// used separated by a ^ character.</param>
        /// <returns>The associated subscription settings.</returns>
        SubscriptionSettings GetSubscriptionConfig(string namespaceName, string settingsKey);
    }
}