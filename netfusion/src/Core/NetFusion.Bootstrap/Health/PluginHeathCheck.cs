using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Health
{
    /// <summary>
    /// Contains the health of a specific plugin based on health of its contained modules.
    /// </summary>
    public class PluginHeathCheck
    {
        private List<ModuleHealthCheck> _moduleHealthChecks = new();
        
        /// <summary>
        /// Plugin associated with the health-check.
        /// </summary>
        public IPlugin Plugin { get; }
        
        /// <summary>
        /// Health-checks of the plugin's modules.
        /// </summary>
        public IEnumerable<ModuleHealthCheck> ModuleHealthChecks => _moduleHealthChecks;

        public PluginHeathCheck(IPlugin plugin)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }
        
        /// <summary>
        /// The overall worst health-check reported for the plugin.  
        /// </summary>
        public HealthCheckResultType OverallHealth => _moduleHealthChecks.Any() ? 
            _moduleHealthChecks.Max(ac => ac.OverallHealth) : HealthCheckResultType.Healthy;

        public void AddModuleHealthCheck(ModuleHealthCheck healthCheck)
        {
            if (healthCheck == null) throw new ArgumentNullException(nameof(healthCheck));
            
            _moduleHealthChecks.Add(healthCheck);
        }
    }
}