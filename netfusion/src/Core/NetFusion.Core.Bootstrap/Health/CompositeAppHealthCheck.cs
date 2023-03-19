using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Core.Bootstrap.Health;

/// <summary>
/// Contains the overall health-check of the composite-application.
/// </summary>
public class CompositeAppHealthCheck
{
    private readonly List<PluginHeathCheck> _pluginHeathChecks = new();

    /// <summary>
    /// All health-checks associated with each plugin form which the composite application is composed.
    /// </summary>
    public IReadOnlyCollection<PluginHeathCheck> PluginHeath { get; }

    public CompositeAppHealthCheck()
    {
        PluginHeath = _pluginHeathChecks;
    }

    public void AddPluginHealthCheck(PluginHeathCheck pluginHeathCheck)
    {
        if (pluginHeathCheck == null) throw new ArgumentNullException(nameof(pluginHeathCheck));
        _pluginHeathChecks.Add(pluginHeathCheck);
    }

    /// <summary>
    /// The overall worst health-check reported for the composite-application.  
    /// </summary>
    public HealthCheckStatusType CompositeAppHealth => _pluginHeathChecks.Any() ?
        _pluginHeathChecks.Max(hc => hc.PluginHealth) : HealthCheckStatusType.Healthy;
}