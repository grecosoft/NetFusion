using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Health
{
    /// <summary>
    /// Contains the overall health-check of the composite application.
    /// </summary>
    public class CompositeAppHealthCheck
    {
        private List<PluginHeathCheck> _pluginHeathChecks = new();

        public void AddPluginHealthCheck(PluginHeathCheck heathCheck)
        {
            if (heathCheck == null) throw new ArgumentNullException(nameof(heathCheck));
            
            _pluginHeathChecks.Add(heathCheck);
        }

        /// <summary>
        /// All health-checks associated with each plugin form which the composite application is composed.
        /// </summary>
        public IEnumerable<PluginHeathCheck> PluginHeathChecks => _pluginHeathChecks;

        /// <summary>
        /// The overall worst health-check reported for the composite-application.  
        /// </summary>
        public HealthCheckResultType OverallHealth => _pluginHeathChecks.Any() ?
            _pluginHeathChecks.Max(hc => hc.OverallHealth) : HealthCheckResultType.Healthy;
    }
}