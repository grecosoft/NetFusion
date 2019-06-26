using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Responsible for coordinating the population of a Service-Collection from a set of plugins.
    /// A ICompositeApp singleton instance is added to the Service-Collection representing the
    /// application composed from the plugins. The host constructs an instance of this class when
    /// bootstrapping the application by calling the CompositeContainer extension method on the
    /// IServiceCollection interface.  The CompositeContainer extension method returns and instance
    /// of ICompositeContainerBuilder providing an Api for registering plugins, setting container
    /// and plugin level configurations, and a Compose method that adds the resulting ICompositeApp
    /// instance to the Service-Collection.  
    /// </summary>
    public class CompositeContainer  
    {
        // Microsoft Service-Collection populated by Plugin Modules:
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;
        
        // Composite Structure:
        private CompositeAppBuilder _compositeAppBuilder;

        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        
        // Container level configurations:
        private readonly List<IContainerConfig> _containerConfigs = new List<IContainerConfig>();
 
        //--------------------------------------------------
        //--Container Initialization
        //--------------------------------------------------
        
        public CompositeContainer(IServiceCollection services, IConfiguration configuration)
        {
            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            AddContainerConfigs();
        }
        
        // Called by CompositeContainerBuilder to add plugins to the composite container.
        // If the plugin type is already registered, the request is ignored.  This allows
        // a plugin to register it's dependent plugins.
        public void RegisterPlugin<T>() where T : IPlugin, new()  
        {
            if (IsPluginRegistered<T>())
            {
                return;
            }
            
            IPlugin plugin = new T();
            _plugins.Add(plugin);
        }
        
        // Updated by unit-tests to add a collection of configured mock plugins.
        public void RegisterPlugins(params IPlugin[] plugins)
        {
            plugins.ForEach(_plugins.Add);
        }
        
        private bool IsPluginRegistered<T>() where T : IPlugin
        {
            return _plugins.Any(p => p.GetType() == typeof(T));
        }
        
        // Add container level configurations that can be updated by the host
        // to control how the container is initialized.
        private void AddContainerConfigs()
        {
            _containerConfigs.Add(new ValidationConfig());
        }
        
        // Returns a container level configuration used to configure the runtime behavior
        // of the built container.
        public T GetContainerConfig<T>() where T : IContainerConfig
        {
            var config = _containerConfigs.FirstOrDefault(c => c.GetType() == typeof(T));
            if (config == null)
            {
                throw new ContainerException(
                    $"Container configuration of type: {typeof(T)} is not registered.");
            }

            return (T)config;
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

            _compositeAppBuilder = new CompositeAppBuilder(_plugins, _configuration, _containerConfigs);
            
            try
            {
//                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapCompose, "Composing Container"))
//                {

                    _compositeAppBuilder.ComposeModules(typeResolver);
                    _compositeAppBuilder.RegisterServices(_serviceCollection);
                    
                    //LogPlugins(_compositeAppBuilder.AllPlugins);
//                }
            }
            catch (ContainerException ex)
            {
                //LogException(ex);
                throw;
            }
            catch (Exception ex)
            {
                //throw LogException(new ContainerException(
                 //   "Unexpected container error.  See Inner Exception.", ex));
            }
        }
        
        
        
     


        

        

        

         

       

        
        
    
        

       



//        private Exception LogException(Exception ex)
//        {
//            _logger.LogErrorDetails(BootstrapLogEvents.BootstrapException, ex, "Bootstrap Exception");
//            return ex;
//        }
//
//        private void LogPlugins(IEnumerable<IPlugin> plugins)
//        {
//            foreach (var plugin in plugins)
//            {
//                _logger.LogTraceDetails(BootstrapLogEvents.BootstrapPluginDetails, "Plug-in", new
//                {
//                    plugin.Name,
//                    plugin.PluginId,
//                    plugin.AssemblyName,
//                    Configs = plugin.Configs.Select(c => c.GetType().FullName),
//                    Modules = plugin.Modules.Select(m => m.GetType().FullName)
//                });
//            }
//        }

        
    }
}