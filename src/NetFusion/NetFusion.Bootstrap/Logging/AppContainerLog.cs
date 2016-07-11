using Autofac.Core;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Takes an instance of the composite application and the built container registry
    /// and produces a nested dictionary structure representing the application that can
    /// be logged during host application initialization as JSON.
    /// </summary>
    internal class AppContainerLog
    {
        private readonly IContainerLogger _logger;
        private readonly  CompositeApplication _application;
        private readonly IComponentRegistry _registry;

        public AppContainerLog(IContainerLogger logger, CompositeApplication application, IComponentRegistry registry)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(application, nameof(application));
            Check.NotNull(registry, nameof(registry));

            _logger = logger;
            _application = application;
            _registry = registry;
        }

        public IDictionary<string, object> GetLog()
        {
            var log = new Dictionary<string, object> { };

            LogFoundPluginAssemblies(log);
            LogNullLoggerMessages(log);
            LogHostApp(log);
            LogAppComponentPlugins(log);
            LogCorePlugins(log);
            return log;
        }

        private void LogFoundPluginAssemblies(IDictionary<string, object> log)
        {
            log["Searched-Plugin-Assemblies"] = new Dictionary<string, object> {
                {"Search-Patterns", _application.SearchPatterns },
                {"AppHost-Assembly", _application.AppHostPlugin.AssemblyName },
                {"AppComponent-Assemblies", _application.AppComponentPlugins.Select(p => p.AssemblyName) },
                {"Core-Assemblies", _application.CorePlugins.Select(p => p.AssemblyName) }
            };
        }

        private void LogNullLoggerMessages(IDictionary<string, object> log)
        {
            var messages = (_logger as NullLogger)?.Messages;
            if (messages != null)
            {
                log["Errors and Warnings"] = messages;
            }
        }

        private void LogHostApp(IDictionary<string, object> log)
        {
            var hostLog = new Dictionary<string, object>();
            log["Application-Host"] = hostLog;

            LogPlugin(_application.AppHostPlugin, hostLog);
        }

        private void LogAppComponentPlugins(IDictionary<string, object> log)
        {
            log["Component-Plugins"] = _application.AppComponentPlugins.Select(plugin =>
            {
                var pluginLog = new Dictionary<string, object>();
                LogPlugin(plugin, pluginLog);
                return pluginLog;
            }).ToDictionary(p => p["Plugin-Name"]);
        }

        private void LogCorePlugins(IDictionary<string, object> log)
        {
            log["Core-Plugins"] = _application.CorePlugins.Select(plugin =>
            {
                var pluginLog = new Dictionary<string, object>();
                LogPlugin(plugin, pluginLog);
                return pluginLog;
            }).ToDictionary(p => p["Plugin-Name"]);
        }

        private void LogPlugin(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin-Name"] = plugin.Manifest.Name;
            log["Plugin-Id"] = plugin.Manifest.PluginId;
            log["Plugin-Assembly"] = plugin.Manifest.AssemblyName;
            log["Plugin-Description"] = plugin.Manifest.Description;

            LogPluginModules(plugin, log);
            LogPluginKnownTypes(plugin, log);
            LogPuginDiscoveredTypes(plugin, log);
            LogPluginRegistrations(plugin, log);
        }

        private void LogPluginModules(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin-Modules"] = plugin.PluginModules.ToDictionary(
                k => k.GetType().FullName,
                pm =>
                {
                    var moduleLog = new Dictionary<string, object>();
                    pm.Log(moduleLog);
                    return new { Log = moduleLog };
                });
        }

        private void LogPluginKnownTypes(Plugin plugin, IDictionary<string, object> log)
        {
            log["Known-Types"] = plugin.PluginTypes
                .Where(pt => pt.IsKnownType)
                .ToDictionary(
                    k => k.Type.FullName.Replace(k.AssemblyName + ".", ""), 
                    v => v.DiscoveredByPlugins.Select(dp => dp.Manifest.Name)); 
        }

        private void LogPuginDiscoveredTypes(Plugin plugin, IDictionary<string, object> log)
        {
            log["Discovered-Types"] = plugin.DiscoveredTypes.Select(kt => kt.Name).ToArray();
        }

        private void LogPluginRegistrations(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin-Services"] = _registry.Registrations
                .Where(r => plugin.HasType(r.Activator.LimitType))
                .GroupBy(
                    k => k.Activator.LimitType.Name,
                    v => new
                    {
                        LifeTime = v.Lifetime.GetType().Name,
                        Services = v.Services.Select(s => s.Description)
                    }).Select(tg => new
                    {
                        Type = tg.Key,
                        Services = tg
                    });
        }
    }
}
