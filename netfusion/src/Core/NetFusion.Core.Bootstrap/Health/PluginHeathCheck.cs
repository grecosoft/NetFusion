using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Health;

/// <summary>
/// Contains the health of a specific plugin based on health of its contained modules.
/// </summary>
public class PluginHeathCheck
{
    private readonly List<ModuleHealthCheck> _moduleHealthChecks = new();
        
    /// <summary>
    /// Plugin type associated with the health-check.
    /// </summary>
    public Type PluginType { get; }
        
    /// <summary>
    /// Health-checks of the plugin's modules.
    /// </summary>
    public IReadOnlyCollection<ModuleHealthCheck> ModuleHealthChecks { get; }

    internal PluginHeathCheck(IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        PluginType = plugin.GetType();
        ModuleHealthChecks = _moduleHealthChecks;
    }
        
    /// <summary>
    /// The overall worst health-check reported for the plugin.  
    /// </summary>
    public HealthCheckStatusType PluginHealth => _moduleHealthChecks.Any() ? 
        _moduleHealthChecks.Max(ac => ac.ModuleHealth) : HealthCheckStatusType.Healthy;

    /// <summary>
    /// Records the health for a module associated with the plugin.
    /// </summary>
    /// <param name="healthCheck">The module health check.</param>
    internal void AddModuleHealthCheck(ModuleHealthCheck healthCheck)
    {
        ArgumentNullException.ThrowIfNull(healthCheck);

        _moduleHealthChecks.Add(healthCheck);
    }
}