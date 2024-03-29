using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Core.Bootstrap.Health;

/// <summary>
/// Contains a set of module associated aspects and information about
/// their current health.  
/// </summary>
public class ModuleHealthCheck
{
    private readonly List<HealthAspectCheck> _aspectsChecks = new();
        
    /// <summary>
    /// The module type associated with the health check.
    /// </summary>
    public Type PluginModuleType { get; }

    /// <summary>
    /// List of associated module aspects and their associated health status.
    /// </summary>
    public IReadOnlyCollection<HealthAspectCheck> AspectChecks { get; }

    /// <summary>
    /// The overall worst health-check reported for a module's associated aspects.
    /// </summary>
    public HealthCheckStatusType ModuleHealth => _aspectsChecks.Any() ?
        _aspectsChecks.Max(ac => ac.HealthCheckStatus) : HealthCheckStatusType.Healthy;
        
    internal ModuleHealthCheck(IModuleHealthCheckProvider pluginModule)
    {
        ArgumentNullException.ThrowIfNull(pluginModule);
        PluginModuleType = pluginModule.GetType();
        AspectChecks = _aspectsChecks;
    }

    /// <summary>
    /// Records information about an aspect of a module with a given status used to determine
    /// its overall health.
    /// </summary>
    /// <param name="aspectCheck">The status associated with a specific aspect of the module.</param>
    public void RecordAspect(HealthAspectCheck aspectCheck)
    {
        ArgumentNullException.ThrowIfNull(aspectCheck);
        _aspectsChecks.Add(aspectCheck);
    }
}