using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Logging;

/// <summary>
/// Takes an instance of the composite application and the built service collection
/// and produces a nested dictionary structure representing the application that can
/// be logged during host application initialization as JSON.
/// </summary>
internal class CompositeAppLogger
{
    private readonly ICompositeAppBuilder _appBuilder;
    private readonly IServiceCollection _services;

    public CompositeAppLogger(ICompositeAppBuilder appBuilder)
    {
        _appBuilder = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder),
            "Composite application builder cannot be null.");

        _services = appBuilder.ServiceCollection;
    }

    public IDictionary<string, object> GetLog()
    {
        var log = new Dictionary<string, object>();

        LogHostApp(log);
        LogAppComponentPlugins(log);
        LogCorePlugins(log);
        return log;
    }

    private void LogHostApp(IDictionary<string, object> log)
    {
        var hostLog = new Dictionary<string, object>();
        log["HostPlugin"] = hostLog;

        LogPlugin(_appBuilder.HostPlugin, hostLog);
    }

    private void LogAppComponentPlugins(IDictionary<string, object> log)
    {
        log["ApplicationPlugins"] = _appBuilder.AppPlugins.Select(plugin =>
        {
            var pluginLog = new Dictionary<string, object>();
            LogPlugin(plugin, pluginLog);
            return pluginLog;
        }).ToDictionary(p => p["PluginId"]);
    }

    private void LogCorePlugins(IDictionary<string, object> log)
    {
        log["CorePlugins"] = _appBuilder.CorePlugins.Select(plugin =>
        {
            var pluginLog = new Dictionary<string, object>();
            LogPlugin(plugin, pluginLog);
            return pluginLog;
        }).ToDictionary(p => p["PluginId"]);
    }

    private void LogPlugin(IPlugin plugin, IDictionary<string, object> log)
    {
        log["PluginId"] = plugin.PluginId;
        log["PluginName"] = plugin.Name;
        log["PluginAssembly"] = plugin.AssemblyName;
        log["PluginDescription"] = plugin.Description;
        log["PluginSourceUrl"] = plugin.SourceUrl;
        log["PluginDocUrl"] = plugin.DocUrl;

        LogPluginModules(plugin, log);
        PluginLogger.LogPluginRegistrations(plugin, _services, log);
    }

    private static void LogPluginModules(IPlugin plugin, IDictionary<string, object> log)
    {
        log["PluginModules"] = plugin.Modules.ToDictionary(
            m => m.GetType().FullName!,
            pm =>
            {
                var moduleLog = new Dictionary<string, object>();
                pm.Log(moduleLog);
                PluginLogger.LogKnownTypeProperties(log, pm);
                return moduleLog;
            });
    }
}