using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

public interface ICompositeContainer
{ 
    /// <summary>
    /// Reference to service collection populated with components created and initialized by plugins.
    /// </summary>
    IServiceCollection ServiceCollection { get; }
    
    /// <summary>
    /// Indicates if the container has been composed from a set of registered plugins.
    /// </summary>
    bool IsComposed { get; } 
    
    /// <summary>
    /// Adds a plugin to the composite-container.  If the plugin type is already registered,
    /// the request is ignored.  This allows a plugin to register it's dependent plugins.
    /// </summary>
    /// <typeparam name="T">The type of the plugin to be added.</typeparam>
    void RegisterPlugin<T>() where T : IPlugin, new();
    
    /// <summary>
    /// Called by unit-tests to add a collection of configured mock plugins.
    /// </summary>
    /// <param name="plugins">The list of plugins to be added.</param>
    void RegisterPlugins(params IPlugin[] plugins);
    
    /// <summary>
    /// Finds a configuration belonging to one of the registered plugins.
    /// </summary>
    /// <typeparam name="T">The type of the IPluginConfig derived configuration.</typeparam>
    /// <returns>Reference to the configuration or an exception if not found.</returns>
    T GetPluginConfig<T>() where T : IPluginConfig;
}