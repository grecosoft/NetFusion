using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Health;

/// <summary>
/// Determines all plugin-modules providing information about the health of the
/// composite application and queries them at runtime to determine the current health.
/// </summary>
internal class HealthCheckBuilder
{
    private readonly ILookup<IPlugin, IModuleHealthCheckProvider> _pluginHealthProviders;
    
    public HealthCheckBuilder(IEnumerable<IPluginModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        // For each plugin, determine which modules provide health-checks:
        _pluginHealthProviders = modules.Select(m => new
        {
            m.Context.Plugin,
            Provider = m as IModuleHealthCheckProvider
        })
        .Where(m => m.Provider != null)
        .ToLookup(m => m.Plugin, m => m.Provider!);
    }

    /// <summary>
    /// Returns the current health of the composite application. 
    /// </summary>
    /// <returns>Details of the composite-application's health.</returns>
    public async Task<CompositeAppHealthCheck> QueryHealthAsync()
    {
        var appHealthCheck = new CompositeAppHealthCheck();

        if (_pluginHealthProviders.Count == 0)
        {
            return appHealthCheck;
        }
        
        foreach (var healthProvider in _pluginHealthProviders)
        {
            var plugin = healthProvider.Key;
            var pluginHealthCheck = new PluginHeathCheck(plugin);
            
            await AddModuleHeathChecks(pluginHealthCheck, healthProvider);
            appHealthCheck.AddPluginHealthCheck(pluginHealthCheck);
        }

        return appHealthCheck;
    }

    private static async Task AddModuleHeathChecks(PluginHeathCheck pluginHealthCheck,
        IEnumerable<IModuleHealthCheckProvider> modules)
    {
        foreach (IModuleHealthCheckProvider module in modules)
        {
            var moduleHealthCheck = new ModuleHealthCheck(module);
            await module.CheckModuleAspectsAsync(moduleHealthCheck);
            pluginHealthCheck.AddModuleHealthCheck(moduleHealthCheck);
        }
    }
}