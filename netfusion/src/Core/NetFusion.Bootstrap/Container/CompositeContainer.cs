using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Instantiated and delegated to by the ICompositeContainerBuilder instance returned by
    /// calling the CompositeContainer extension method on IServiceCollection.  The methods
    /// contained on the IContainerBuilder interface are used to register plugins with the
    /// CompositeContainer.
    ///
    /// The composite-container builder is used when configuring the Generic Host and allows
    /// for adding services to the IServiceCollection using a controlled process common across
    /// all plugins.  A plugin's associated modules are called to register services provided by
    /// the plugin when the Compose method of IContainerBuilder is called.  After the Compose
    /// method is called, an ICompositeApp singleton instance is added to the service-collection.
    /// </summary>
    public class CompositeContainer 
    {
        // Microsoft Service-Collection populated by Plugin Modules:
        private readonly IServiceCollection _serviceCollection;
        
        // Composite Structure:
        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        private readonly CompositeAppBuilder _builder;

        public bool IsComposted { get; private set; }

        //--------------------------------------------------
        //--Container Initialization
        //--------------------------------------------------

        public ICompositeAppBuilder AppBuilder => _builder;
        public IBootstrapLogger BootstrapLogger => _builder.BootstrapLogger;
        
        // Instantiated by CompositeContainerBuilder.
        public CompositeContainer(IServiceCollection services, IConfiguration configuration)
        {
            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));

            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _builder = new CompositeAppBuilder(services, configuration);
            
            AddContainerConfigs();
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
        /// Updated by unit-tests to add a collection of configured mock plugins.
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
        
        // Add container level configurations that can be updated by the host
        // to control how the composite-application is initialized.
        private void AddContainerConfigs()
        {
            _builder.AddContainerConfig(new ValidationConfig());
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
            var configs = _plugins.SelectMany(p => p.Configs)
                .Where(c => c.GetType() == typeof(T)).ToArray();

            if (configs.Length > 1)
            {
                throw new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} has been registered by multiple plugins." +
                    "A configuration can be registered by only one plugin.");
            }

            if (configs.Length == 0)
            {
                throw new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} is not registered.");
            }

            return (T)configs.First();
        }
        
        
        //--------------------------------------------------
        //--Container Composition
        //--------------------------------------------------
        
        public void Compose(ITypeResolver typeResolver)
        {
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));

            if (IsComposted)
            {
                throw new ContainerException("Container has already been composed.");
            }

            try
            {
                // Delegate to the builder:
                _builder.ComposeModules(typeResolver, _plugins);
                _builder.RegisterServices(_serviceCollection);

                IsComposted = true;

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

        private Exception LogException(Exception ex)
        {
            _builder.BootstrapLogger.Add(LogLevel.Error, $"Bootstrap Exception: {ex}");
            return ex;
        }
    }
}