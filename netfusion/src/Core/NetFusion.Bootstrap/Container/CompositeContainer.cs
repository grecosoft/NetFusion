using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Manages a collection of plugins and initializes an instance of ICompositeAppBuilder
    /// delegated to for creating an instance of ICompositeApp bootstrapped from the set of
    /// plugins.
    /// </summary>
    public class CompositeContainer 
    {
        // Microsoft Service-Collection populated by Plugin Modules:
        private readonly IServiceCollection _serviceCollection;
        
        // Composite Structure:
        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        private readonly CompositeAppBuilder _builder;

        public bool IsComposed { get; private set; }

        //--------------------------------------------------
        //--Container Initialization
        //--------------------------------------------------

        public ICompositeAppBuilder AppBuilder => _builder;
        
        // Instantiated by CompositeContainerBuilder.
        public CompositeContainer(IServiceCollection services, 
            IConfiguration configuration,
            IBootstrapLogger bootstrapLogger)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (bootstrapLogger == null) throw new ArgumentNullException(nameof(bootstrapLogger));

            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));
            _builder = new CompositeAppBuilder(services, configuration, bootstrapLogger);
        }
        
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
            plugins.ForEach(_plugins.Add);
        }
        
        private bool IsPluginRegistered<T>() where T : IPlugin
        {
            return _plugins.Any(p => p.GetType() == typeof(T));
        }

        // Returns a container level configuration used to configure the runtime behavior
        // of the built container.
        public T GetContainerConfig<T>() where T : IContainerConfig
        {
            return _builder.GetContainerConfig<T>();
        }
        
        // Finds a configuration belonging to one of the registered plugins.  When a plugin
        // is registered with the container, it can extend the behavior of another plugin by
        // requesting a configuration from the other plugin and setting information used to
        // extended the base implementation.
        public T GetPluginConfig<T>() where T : IPluginConfig
        {
            var pluginConfig = _plugins.SelectMany(p => p.Configs)
                .FirstOrDefault(c => c.GetType() == typeof(T));
            
            if (pluginConfig == null)
            {
                throw new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} is not registered.");
            }

            return (T) pluginConfig;
        }
        
        
        //--------------------------------------------------
        //--Container Composition
        //--------------------------------------------------
        
        public void Compose(ITypeResolver typeResolver)
        {
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            
            try
            {
                if (IsComposed)
                {
                    throw new ContainerException("Container has already been composed.");
                }
                
                // Delegate to the builder:
                _builder.ComposeModules(typeResolver, _plugins);
                _builder.RegisterServices(_serviceCollection);

                IsComposed = true;

            }
            catch (ContainerException ex)
            {
                LogException(ex);
                throw;
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Unexpected container error.  See Inner Exception.", ex));
            }
        }

        private Exception LogException(ContainerException ex)
        {
            if (ex.Details != null)
            {
                _builder.BootstrapLogger.Add(LogLevel.Error, 
                    $"Bootstrap Exception: {ex}; Details: {ex.Details.ToIndentedJson()}");
                return ex;
            }
            
            _builder.BootstrapLogger.Add(LogLevel.Error, $"Bootstrap Exception: {ex}");
            return ex;
        }
    }
}