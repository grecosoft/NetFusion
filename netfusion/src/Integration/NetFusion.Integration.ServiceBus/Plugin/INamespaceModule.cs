using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Plugin.Configs;

namespace NetFusion.Integration.ServiceBus.Plugin;

/// <summary>
/// Module responsible for managing Namespace connections and configurations.
/// </summary>
public interface INamespaceModule : IPluginModuleService
{
    /// <summary>
    /// Plugin configuration provided when microservice is bootstrapped.
    /// </summary>
    ServiceBusConfig BusPluginConfiguration { get; }

    /// <summary>
    /// Returns the connection for a given namespace.
    /// </summary>
    /// <param name="namespaceName">The namespace to fine corresponding connection.</param>
    /// <returns>The connection if found.  Otherwise, an exception is raised.</returns>
    NamespaceConnection GetConnection(string namespaceName);
}