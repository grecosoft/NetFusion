using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Logging;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Logging;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Manages a collection of plugins and initializes an instance of CompositeAppBuilder
/// delegated to for creating an instance of CompositeApp bootstrapped from the set of
/// plugins.
/// </summary>
public class CompositeContainer 
{
    // Microsoft Service-Collection populated by Plugin Modules:
    private readonly IServiceCollection _serviceCollection;
        
    // Composite Structure:
    private readonly List<IPlugin> _plugins = new();
    private readonly CompositeAppBuilder _builder;

    /// <summary>
    /// Indicates if the container has been composed from a set of registered plugins.
    /// </summary>
    public bool IsComposed { get; private set; }

    // --------------------------- [Container Initialization] -------------------------------

    // Instantiated by CompositeContainerBuilder.
    public CompositeContainer(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        _builder = new CompositeAppBuilder(serviceCollection, configuration);
    }
        
    /// <summary>
    /// Reference to ICompositeAppBuilder responsible for building an instance of CompositeApp
    /// from a list of registered plugins.
    /// </summary>
    public ICompositeAppBuilder AppBuilder => _builder;
        
    /// <summary>
    /// Adds a plugin to the composite-container.  If the plugin type is already registered,
    /// the request is ignored.  This allows a plugin to register it's dependent plugins.
    /// </summary>
    /// <typeparam name="T">The type of the plugin to be added.</typeparam>
    public void RegisterPlugin<T>() where T : IPlugin, new()  
    {
        if (IsPluginRegistered<T>())
        {
            return;
        }
            
        IPlugin plugin = new T();
        _plugins.Add(plugin);
    }
        
    /// <summary>
    /// Called by unit-tests to add a collection of configured mock plugins.
    /// </summary>
    /// <param name="plugins">The list of plugins to be added.</param>
    public void RegisterPlugins(params IPlugin[] plugins) => 
        _plugins.AddRange(plugins);
    
    private bool IsPluginRegistered<T>() where T : IPlugin =>
        _plugins.Any(p => p.GetType() == typeof(T));

    // --------------------------- [Configurations] -------------------------------

    /// <summary>
    /// Finds a configuration belonging to one of the registered plugins.
    /// </summary>
    /// <typeparam name="T">The type of the IPluginConfig derived configuration.</typeparam>
    /// <returns>Reference to the configuration or an exception if not found.</returns>
    public T GetPluginConfig<T>() where T : IPluginConfig
    {
        var pluginConfig = _plugins.SelectMany(p => p.Configs)
            .FirstOrDefault(c => c.GetType() == typeof(T));
            
        if (pluginConfig == null)
        {
            throw new BootstrapException(
                $"Plugin configuration of type: {typeof(T)} is not registered.", "missing-plugin-config");
        }

        return (T) pluginConfig;
    }

    // --------------------------- [Container Composition] -------------------------------

    /// <summary>
    /// Called by CompositeContainerBuilder to build an instance of CompositeApp from the 
    /// list of registered plugins.
    /// </summary>
    /// <param name="typeResolver">Reference to an implementation responsible for resolving
    /// a plugin's types.</param>
    public void Compose(ITypeResolver typeResolver)
    {
        if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));

        string version = GetType().Assembly.GetName().Version?.ToString() ?? string.Empty;
            
        try
        {
            if (IsComposed)
            {
                throw new BootstrapException("Container already composed", "already-composed");
            }

            NfExtensions.Logger.Log<CompositeContainer>(LogLevel.Information, 
                "NetFusion {Version} Bootstrapping", version);
                
            // Delegate to the builder:
            _builder.AssemblePlugins(_plugins.ToArray(), typeResolver);
            _builder.RegisterPluginServices();

            LogComposedPlugins(_plugins, _serviceCollection);

            IsComposed = true;
        }
        catch (BootstrapException ex)
        {
            NfExtensions.Logger.LogError<CompositeContainer>(ex, "Bootstrap Exception");
            throw;
        }
        catch (Exception ex)
        {
            NfExtensions.Logger.LogError<CompositeContainer>(ex, "Bootstrap Exception");
            throw new BootstrapException("Unexpected container error.  See Inner Exception.", ex);
        }
    }

    private static void LogComposedPlugins(IEnumerable<IPlugin> plugins, IServiceCollection services)
    {
        foreach (var plugin in plugins)
        {
            LogMessage pluginLog = PluginLogger.Log(plugin, services);
            NfExtensions.Logger.Log<CompositeContainer>(pluginLog);
        }
    }
}