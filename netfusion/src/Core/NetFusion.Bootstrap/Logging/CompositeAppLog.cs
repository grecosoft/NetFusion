using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Takes an instance of the composite application and the built service collection
    /// and produces a nested dictionary structure representing the application that can
    /// be logged during host application initialization as JSON.
    /// </summary>
    public class CompositeAppLog
    {
        private readonly ICompositeAppBuilder _application;
        private readonly IServiceCollection _services;

        public CompositeAppLog(ICompositeAppBuilder application, IServiceCollection services)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application),
                "Composite Application to log cannot be null.");

            _services = services ?? throw new ArgumentNullException(nameof(services),
                "Service collection to log cannot be null.");
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

            LogPlugin(_application.HostPlugin, hostLog);
        }

        private void LogAppComponentPlugins(IDictionary<string, object> log)
        {
            log["ApplicationPlugins"] = _application.AppPlugins.Select(plugin =>
            {
                var pluginLog = new Dictionary<string, object>();
                LogPlugin(plugin, pluginLog);
                return pluginLog;
            }).ToDictionary(p => p["PluginId"].ToString());
        }

        private void LogCorePlugins(IDictionary<string, object> log)
        {
            log["CorePlugins"] = _application.CorePlugins.Select(plugin =>
            {
                var pluginLog = new Dictionary<string, object>();
                LogPlugin(plugin, pluginLog);
                return pluginLog;
            }).ToDictionary(p => p["PluginId"].ToString());
        }

        private void LogPlugin(IPlugin plugin, IDictionary<string, object> log)
        {
            log["PluginName"] = plugin.Name;
            log["PluginId"] = plugin.PluginId;
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
                m => m.GetType().FullName,
                pm =>
                {
                    var moduleLog = new Dictionary<string, object>();
                    pm.Log(moduleLog);
                    return moduleLog;
                });
        }
    }
}
