using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Health
{
    /// <summary>
    /// Contains a set of module associated aspects and information about
    /// their current health.  
    /// </summary>
    public class ModuleHealthCheck
    {
        private List<HealthAspectCheck> _aspectsChecks = new();
        
        /// <summary>
        /// The module associated with the health check.
        /// </summary>
        public IPluginModule PluginModule { get; }

        /// <summary>
        /// List of associated module aspects and their associated health status.
        /// </summary>
        public IEnumerable<HealthAspectCheck> AspectChecks => _aspectsChecks;

        /// <summary>
        /// The overall worst health-check reported for a module's associated aspects.
        /// </summary>
        public HealthCheckResultType OverallHealth => _aspectsChecks.Any() ?
            _aspectsChecks.Max(ac => ac.CheckResult) : HealthCheckResultType.Healthy;
        

        public ModuleHealthCheck(IPluginModule pluginModule)
        {
            PluginModule = pluginModule ?? throw new ArgumentNullException(nameof(pluginModule));
        }

        /// <summary>
        /// Records information about an aspect of a module, used to determine it overall health,
        /// having a currently healthy status.
        /// </summary>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordHealthyAspect(string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = HealthCheckResultType.Healthy
            }.ThrowIfInvalid());
        }

        /// <summary>
        /// Records information about an aspect of a module, used to determine it overall health,
        /// having a currently degraded status.
        /// </summary>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordedDegradedAspect(string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = HealthCheckResultType.Degraded
            }.ThrowIfInvalid());
        }

        /// <summary>
        /// Records information about an aspect of a module, used to determine it overall health,
        /// having a currently unhealthy status.
        /// </summary>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordedUnhealthyAspect(string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = HealthCheckResultType.Unhealthy
            }.ThrowIfInvalid());
        }
    }
}