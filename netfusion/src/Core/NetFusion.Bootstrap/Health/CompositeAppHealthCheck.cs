using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Health
{
    /// <summary>
    /// Contains the overall health-check of the composite-application.
    /// </summary>
    public class CompositeAppHealthCheck
    {
        private readonly List<PluginHeathCheck> _pluginHeathChecks = new();

        private CompositeAppHealthCheck() { }

        /// <summary>
        /// Creates an instance of the composite-application health check based on the plugins
        /// from which it was built.  Each plugin module implementing IModuleHealthCheck is
        /// called and allowed to report the status of any services it manages.
        /// </summary>
        /// <param name="plugins">List of all plugins from which the composite application was built.</param>
        /// <returns>Current health of the composite-application.</returns>
        public static async Task<CompositeAppHealthCheck> QueryHealthAsync(IEnumerable<IPlugin> plugins)
        {
            if (plugins == null) throw new ArgumentNullException(nameof(plugins));
            
            var healthCheck = new CompositeAppHealthCheck();
            
            foreach (IPlugin plugin in plugins)
            {
                var moduleHealthChecks = plugin.Modules.OfType<IModuleHealthCheck>().ToArray();
                if (! moduleHealthChecks.Any())
                {
                    // Plugin does not have any modules supporting health-checks.
                    continue;
                }

                // Create a plugin health-check and populate it with health-checks pertaining
                // to each of its modules.
                var pluginHealthCheck = new PluginHeathCheck(plugin);
                healthCheck._pluginHeathChecks.Add(pluginHealthCheck);
                
                foreach (IModuleHealthCheck module in moduleHealthChecks)
                {
                    var moduleHealthCheck = new ModuleHealthCheck(module);
                    await module.CheckModuleAspectsAsync(moduleHealthCheck);
                    pluginHealthCheck.AddModuleHealthCheck(moduleHealthCheck);
                }
            }

            return healthCheck;
        }

        /// <summary>
        /// All health-checks associated with each plugin form which the composite application is composed.
        /// </summary>
        public IEnumerable<PluginHeathCheck> PluginHeathChecks => _pluginHeathChecks;

        /// <summary>
        /// The overall worst health-check reported for the composite-application.  
        /// </summary>
        public HealthCheckStatusType CompositeAppHealth => _pluginHeathChecks.Any() ?
            _pluginHeathChecks.Max(hc => hc.PluginHealth) : HealthCheckStatusType.Healthy;
    }
}