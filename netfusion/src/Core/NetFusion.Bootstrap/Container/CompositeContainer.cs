using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap.Exceptions;
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

        // --------------------------- [Container Initialization] -------------------------------
  
        public ICompositeAppBuilder AppBuilder => _builder;
        
        // Instantiated by CompositeContainerBuilder.
        public CompositeContainer(IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));
            _builder = new CompositeAppBuilder(services, configuration);
        }

        private string Version => GetType().Assembly.GetName().Version.ToString();
        
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
        
        // --------------------------- [Configurations] -------------------------------
        
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

                NfExtensions.Logger.Add(LogLevel.Information, "NetFusion {Version}", Version);
                
                // Delegate to the builder:
                _builder.ComposeModules(typeResolver, _plugins);
                _builder.RegisterServices(_serviceCollection);

                LogComposedPlugins();

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
        
        // --------------------------- [Logging] -------------------------------

        // Creates a log message for each plug-in.  Then adds details pertaining
        // to the plug-in as log properties.  
        private void LogComposedPlugins()
        {
            foreach (LogMessage pluginLog in _plugins.Select(CreatePluginLog))
            {
                NfExtensions.Logger.Write(LogLevel.Information, pluginLog);
            }
        }

        private static LogMessage CreatePluginLog(IPlugin plugin)
        {
            var logMessage = LogMessage.For("{PluginType} {Name} Composed", plugin.PluginType, plugin.Name);
            LogPluginMetadata(logMessage, plugin);
            LogPluginModules(logMessage, plugin);

            return logMessage;
        }

        public static void LogPluginMetadata(LogMessage logMessage, IPlugin plugin)
        {
            logMessage.WithProperties(
                new LogProperty { Name = "PluginId", Value = plugin.PluginId },
                new LogProperty { Name = "Assembly", Value = plugin.AssemblyName },
                new LogProperty { Name = "Version", Value = plugin.AssemblyVersion },
                new LogProperty { Name = "Description", Value = plugin.Description },
                new LogProperty { Name = "SourceUrl", Value = plugin.SourceUrl }
            );
        }

        // Adds a log property named Modules containing a log for each plug-in module.
        public static void LogPluginModules(LogMessage logMessage, IPlugin plugin)
        {
            var moduleLogs = plugin.Modules.Select(m =>
            {
                var moduleLog = new Dictionary<string, object>();
                m.Log(moduleLog);
                
                LogDependentModules(moduleLog, m);
                LogKnownTypeProperties(moduleLog, m);
                
                return new { m.Name, moduleLog };
            }).ToDictionary(i => i.Name);

            logMessage.WithProperties(new LogProperty {
                Name = "Modules", 
                Value = moduleLogs, 
                DestructureObjects = true
            });
        }

        private static void LogDependentModules(IDictionary<string, object> values, IPluginModule module)
        {
            var dependencies = module.DependentServiceModules.Select(dms => new {
                ModuleProperty = dms.Name,
                ReferencedModule = dms.PropertyType.FullName
            });

            values["DependentModules"] = dependencies.ToArray();
        }

        private static void LogKnownTypeProperties(IDictionary<string, object> values, IPluginModule module)
        {
            var discoveredProps = module.KnownTypeProperties.Select(kt => new
            {
                PropertyName = kt.Key.Name,
                KnownType = kt.Value.Item1,
                DiscoveredInstances = kt.Value.Item2.Select(t => t.FullName)
            });

            values["DiscoveredProperties"] = discoveredProps.ToArray();
        }

        private static Exception LogException(ContainerException ex)
        {
            if (ex.Details != null)
            {
                NfExtensions.Logger.Add(LogLevel.Error, 
                    $"Bootstrap Exception: {ex}; Details: {ex.Details.ToIndentedJson()}");
                return ex;
            }
            
            NfExtensions.Logger.Add(LogLevel.Error, $"Bootstrap Exception: {ex}");
            return ex;
        }
    }
}