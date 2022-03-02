using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Contains methods specific to logging plugin details.
    /// </summary>
    public static class PluginLogger
    {
        /// <summary>
        /// Creates a log message for each plug-in and adds details pertaining to the plug-in as log properties.  
        /// </summary>
        /// <param name="plugin">Plugin from which the composite application is composed.</param>
        /// <param name="services">Reference to the dependency-injection service collection.</param>
        public static LogMessage Log(IPlugin plugin, IServiceCollection services)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            var logMessage = LogMessage.For(LogLevel.Information, "{PluginType} {Name} Composed", 
                plugin.PluginType, plugin.Name);
                
            LogPluginMetadata(logMessage, plugin);
            LogPluginModules(logMessage, plugin);
            LogPluginServices(logMessage, plugin, services);

            return logMessage;
        }

        private static void LogPluginMetadata(LogMessage logMessage, IPlugin plugin)
        {
            logMessage.WithProperties(
                new LogProperty { Name = "PluginId", Value = plugin.PluginId },
                new LogProperty { Name = "Assembly", Value = plugin.AssemblyName },
                new LogProperty { Name = "Version", Value = plugin.AssemblyVersion },
                new LogProperty { Name = "DocUrl", Value = plugin.DocUrl },
                new LogProperty { Name = "SourceUrl", Value = plugin.SourceUrl }
            );
        }
        
        // Adds a log property named Modules containing a log for each plug-in module.
        private static void LogPluginModules(LogMessage logMessage, IPlugin plugin)
        {
            var moduleLogs = plugin.Modules.Select(m =>
            {
                var moduleLog = new Dictionary<string, object>();
                m.Log(moduleLog);
                
                LogDependentModules(moduleLog, m);
                LogKnownTypeProperties(moduleLog, m);
                
                return new { m.Name, moduleLog };
            }).ToDictionary(i => i.Name);

            logMessage.WithProperties(
                new LogProperty { Name = "Modules", Value = moduleLogs});
        }

        private static void LogDependentModules(IDictionary<string, object> values, IPluginModule module)
        {
            var dependencies = module.DependentServiceModules.Select(dsm => new {
                ModuleProperty = dsm.Name,
                ReferencedModule = dsm.PropertyType.FullName
            });

            values["DependentModules"] = dependencies.ToArray();
        }

        public static void LogKnownTypeProperties(IDictionary<string, object> values, IPluginModule module)
        {
            var discoveredProps = module.KnownTypeProperties.Select(kt => new
            {
                PropertyName = kt.Key.Name,
                KnownType = kt.Value.Item1.FullName,
                DiscoveredInstances = kt.Value.Item2.Select(t => t.FullName)
            });

            values["DiscoveredProperties"] = discoveredProps.ToArray();
        }
        
        private static void LogPluginServices(LogMessage logMessage, IPlugin plugin, IServiceCollection services)
        {
            var serviceLog = new Dictionary<string, object>();
            
            LogPluginRegistrations(plugin, services, serviceLog);
            
            logMessage.WithProperties(new LogProperty {
                Name = "RegisteredServices", 
                Value = serviceLog
            });
        }

        /// <summary>
        /// Adds to a log the services registered by a specific plugin with IServiceCollection.
        /// </summary>
        /// <param name="plugin">The plugin used to lookup service registrations.</param>
        /// <param name="services">The populated service collection.</param>
        /// <param name="log">The log to which the associated registrations should be added.</param>
        public static void LogPluginRegistrations(IPlugin plugin, IServiceCollection services,
            IDictionary<string, object> log)
        {
            var implementationTypes = services.Select(s => new {
                s.ServiceType,
                ImplementationType = s.ImplementationType ?? s.ImplementationInstance?.GetType(),
                LifeTime = s.Lifetime.ToString(),
                IsFactory = s.ImplementationFactory != null
            });

            // Logs the service implementations defined within the plugin registered
            // for a given service type.
            log["Registrations"] = implementationTypes
                .Where(it => !it.IsFactory && plugin.HasType(it.ImplementationType))
                .Select(rt => new
                {
                    ServiceType = rt.ServiceType.FullName,
                    ImplementationType = rt.ImplementationType.FullName,
                    rt.LifeTime
                }).OrderBy(rt => rt.ServiceType).ToArray();
        }
    }
}