using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Core.Bootstrap.Exceptions;

namespace NetFusion.Core.Bootstrap.Plugins;

/// <summary>
/// Base class providing a default implementation of the IPlugin interface.
/// Each assembly defining a plugin should define a derived type containing
/// metadata, configurations, and modules from which the plugin is comprised.
/// </summary>
public abstract class PluginBase : IPlugin
{
    // Properties that must be specified by derived plugins:
    public abstract string PluginId { get; }
    public abstract PluginTypes PluginType { get; }
    public abstract string Name { get; }

    // Optional Properties that can be specified by derived plugins.
    public string Description { get; protected set; } = string.Empty;
    public string SourceUrl { get; protected set; } = string.Empty;
    public string DocUrl { get; protected set; } = string.Empty;

    // Plugin Parts:
    public IEnumerable<IPluginConfig> Configs => _configs;
    public IEnumerable<IPluginModule> Modules => _modules;
        
    // Assembly Metadata set by ITypeResolver:
    public string AssemblyName { get; protected set; } = string.Empty;
    public string AssemblyVersion { get; protected set; } = string.Empty;
    public IEnumerable<Type> Types { get; protected set; } = Enumerable.Empty<Type>();

    private readonly List<IPluginConfig> _configs = new();
    private readonly List<IPluginModule> _modules = new();

    /// <summary>
    /// Adds a module to the plugin.
    /// </summary>
    /// <typeparam name="TModule">The module type.</typeparam>
    protected void AddModule<TModule>() where TModule : IPluginModule, new()
    {
        if (_modules.Any(m => m.GetType() == typeof(TModule)))
        {
            throw new BootstrapException($"Plugin Module of type: {typeof(TModule)} already added to plugin type: {GetType()}.", 
                "bootstrap-duplicate-module");
        }
            
        _modules.Add(new TModule());
    }

    /// <summary>
    /// Adds a configuration to the plugin.
    /// </summary>
    /// <typeparam name="TConfig">The configuration type.</typeparam>
    protected void AddConfig<TConfig>() where TConfig : IPluginConfig, new()
    {
        if (_configs.Any(c => c.GetType() == typeof(TConfig)))
        {
            throw new BootstrapException($"Plugin Configuration of type: {typeof(TConfig)} already added to plugin type: {GetType()}.", 
                "bootstrap-duplicate-config");
        }
            
        _configs.Add(new TConfig());
    }

    public void SetPluginMeta(string assemblyName, string assemblyVersion, 
        IEnumerable<Type> pluginTypes)
    {
        AssemblyName = assemblyName;
        AssemblyVersion = assemblyVersion;
        Types = pluginTypes;
    }

    public bool HasType(Type pluginType)
    {
        ArgumentNullException.ThrowIfNull(pluginType);
        return Types.Any(pt => pt == pluginType);
    }
        
    public T GetConfig<T>() where T : IPluginConfig
    {
        var config = _configs.FirstOrDefault(
            pc => pc.GetType() == typeof(T));

        if (config == null)
        {
            throw new BootstrapException(
                $"Plugin configuration of type: {typeof(T)} is not registered for plugin type: {GetType()}",
                "missing-plugin-config");
        }

        return (T)config;
    }

    public async Task StartAsync(ILogger logger, IServiceProvider services)
    {
        foreach (var module in Modules)
        {
            logger.LogDebug("Starting Module: {moduleType} for Plugin: {pluginName}",
                module.Name,
                module.Context.Plugin.Name);

            await module.StartModuleAsync(services);
        }
    }

    public async Task RunAsync(ILogger logger, IServiceProvider services)
    {
        foreach (var module in Modules)
        {
            logger.LogDebug("Running Module: {moduleType} for Plugin: {pluginName}",
                module.Name,
                module.Context.Plugin.Name);

            await module.RunModuleAsync(services);
        }
    }

    public async Task StopAsync(ILogger logger, IServiceProvider services)
    {
        foreach (var module in Modules.Reverse())
        {
            logger.LogDebug("Stopping Module: {moduleType} for Plugin: {pluginName}",
                module.Name,
                module.Context.Plugin.Name);

            await module.StopModuleAsync(services);
        }
    }
}