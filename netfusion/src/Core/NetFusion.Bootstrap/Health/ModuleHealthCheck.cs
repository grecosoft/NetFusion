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
        public HealthCheckStatusType OverallHealth => _aspectsChecks.Any() ?
            _aspectsChecks.Max(ac => ac.CheckResult) : HealthCheckStatusType.Healthy;
        

        public ModuleHealthCheck(IPluginModule pluginModule)
        {
            PluginModule = pluginModule ?? throw new ArgumentNullException(nameof(pluginModule));
        }

        /// <summary>
        /// Records information about an aspect of a module with a given status used to determine
        /// its overall health.
        /// </summary>
        /// <param name="healthCheckStatus">The associated status.</param>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordAspect(HealthCheckStatusType healthCheckStatus, string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = healthCheckStatus
            }.ThrowIfInvalid());
        }

        /// <summary>
        /// Records information about a healthy aspect of a module used to determine its overall health.
        /// </summary>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordHealthyAspect(string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = HealthCheckStatusType.Healthy
            }.ThrowIfInvalid());
        }

        /// <summary>
        /// Records information about a degraded aspect of a module used to determine its overall health.
        /// </summary>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordedDegradedAspect(string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = HealthCheckStatusType.Degraded
            }.ThrowIfInvalid());
        }

        /// <summary>
        /// Records information about a unhealthy aspect of a module used to determine its overall health.
        /// </summary>
        /// <param name="name">The name used to identify an aspect of a module.</param>
        /// <param name="value">The value describing the aspect.</param>
        public void RecordedUnhealthyAspect(string name, string value)
        {
            _aspectsChecks.Add(new HealthAspectCheck
            {
                AspectName = name, 
                AspectValue = value,
                CheckResult = HealthCheckStatusType.Unhealthy
            }.ThrowIfInvalid());
        }
    }
}