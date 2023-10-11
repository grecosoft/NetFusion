using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Logging;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Logging;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Manages a collection of plugins and initializes an instance of CompositeAppBuilder
/// delegated to for creating an instance of CompositeApp bootstrapped from the set of
/// plugins.
///
/// https://github.com/grecosoft/NetFusion/wiki/core-bootstrap-overview
/// </summary>
internal class CompositeContainer : ICompositeContainer 
{
    private readonly ILogger<CompositeContainer> _bootstrapLogger;
    
    private readonly List<IPlugin> _plugins = new();
    private readonly CompositeAppBuilder _builder;
    
    public IServiceCollection ServiceCollection { get; }
    public bool IsComposed { get; private set; }
    
    // --------------------------- [Container Initialization] -------------------------------

    // Instantiated by CompositeContainerBuilder.
    public CompositeContainer(IServiceCollection serviceCollection, 
        ILoggerFactory bootstrapLoggerFactory, 
        IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        
        _bootstrapLogger = bootstrapLoggerFactory.CreateLogger<CompositeContainer>();
        _builder = new CompositeAppBuilder(serviceCollection, bootstrapLoggerFactory, configuration);
    }
        
    /// <summary>
    /// Reference to ICompositeAppBuilder responsible for building an instance of CompositeApp
    /// from a list of registered plugins.
    /// </summary>
    public ICompositeAppBuilder AppBuilder => _builder;
    
    public void RegisterPlugin<T>() where T : IPlugin, new()  
    {
        if (IsPluginRegistered<T>())
        {
            return;
        }
            
        IPlugin plugin = new T();
        _plugins.Add(plugin);
    }
    
    public void RegisterPlugins(params IPlugin[] plugins) => 
        _plugins.AddRange(plugins);
    
    private bool IsPluginRegistered<T>() where T : IPlugin =>
        _plugins.Any(p => p.GetType() == typeof(T));

    // --------------------------- [Configurations] -------------------------------
    
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
        
        try
        {
            if (IsComposed)
            {
                throw new BootstrapException("Container already composed", "already-composed");
            }
            
            string version = GetType().Assembly.GetName().Version?.ToString() ?? string.Empty;

            _bootstrapLogger.LogInformation("NetFusion {Version} Bootstrapping", version);
                
            // Delegate to the builder:
            _builder.AssemblePlugins(_plugins.ToArray(), typeResolver);
            _builder.RegisterPluginServices();

            LogComposedPlugins(_plugins, ServiceCollection);

            IsComposed = true;
        }
        catch (BootstrapException ex)
        {
            _bootstrapLogger.LogError(ex, "Bootstrap Exception");
            throw;
        }
        catch (Exception ex)
        {
            _bootstrapLogger.LogError(ex, "Bootstrap Exception");
            throw new BootstrapException("Unexpected container error.  See Inner Exception.", ex);
        }
    }

    private void LogComposedPlugins(IEnumerable<IPlugin> plugins, IServiceCollection services)
    {
        foreach (var plugin in plugins)
        {
            LogMessage pluginLog = PluginLogger.Log(plugin, services);
            _bootstrapLogger.Log(pluginLog);
        }
    }
}