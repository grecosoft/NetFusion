using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
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

        public bool IsComposed { get; private set; }

        // --------------------------- [Container Initialization] -------------------------------

        // Instantiated by CompositeContainerBuilder.
        public CompositeContainer(IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));
            _builder = new CompositeAppBuilder(services, configuration);
            
            AddContainerConfigurations(_builder);
        }
        
        public ICompositeAppBuilder AppBuilder => _builder;

        private string Version => GetType().Assembly.GetName().Version?.ToString() ?? string.Empty;
        
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
        public void RegisterPlugins(params IPlugin[] plugins)
        {
            _plugins.AddRange(plugins);
        }
        
        private bool IsPluginRegistered<T>() where T : IPlugin
        {
            return _plugins.Any(p => p.GetType() == typeof(T));
        }
        
        // --------------------------- [Configurations] -------------------------------

        // Finds a configuration belonging to one of the registered plugins.
        public T GetPluginConfig<T>() where T : IPluginConfig
        {
            var pluginConfig = _plugins.SelectMany(p => p.Configs)
                .FirstOrDefault(c => c.GetType() == typeof(T));
            
            if (pluginConfig == null)
            {
                throw new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} is not registered.", "missing-plugin-config");
            }

            return (T) pluginConfig;
        }

        // This method is called from the constructor and is a placeholder for future
        // implementations if global settings pertaining to CompositeContainer can be
        // overridden by the application host.
        private static void AddContainerConfigurations(ICompositeAppBuilder builder)
        {
            // The code here should create instances of IContainerConfig configuration
            // instances with default settings specified.  The configuration should then
            // be registered by calling: builder.AddContainerConfig(config);
            
            // Then the host calls GetContainerConfig to obtain the default configuration
            // to which it can override any default configurations.
        }
        
        // --------------------------- [Container Composition] -------------------------------
        
        // Called by CompositeContainerBuilder to build an instance of CompositeApp
        // from the list of registered plugins.  
        public void Compose(ITypeResolver typeResolver)
        {
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            
            try
            {
                if (IsComposed)
                {
                    throw new ContainerException("Container already composed");
                }

                NfExtensions.Logger.Log<CompositeContainer>(LogLevel.Information, 
                    "NetFusion {Version} Bootstrapping", Version);
                
                // Delegate to the builder:
                _builder.ComposePlugins(typeResolver, _plugins.ToArray());
                _builder.RegisterPluginServices(_serviceCollection);

                LogComposedPlugins(_plugins, _serviceCollection);

                IsComposed = true;
            }
            catch (ContainerException ex)
            {
                NfExtensions.Logger.LogError<CompositeContainer>(ex, "Bootstrap Exception");
                throw;
            }
            catch (Exception ex)
            {
                NfExtensions.Logger.LogError<CompositeContainer>(ex, "Bootstrap Exception");
                throw new ContainerException("Unexpected container error.  See Inner Exception.", ex);
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
}