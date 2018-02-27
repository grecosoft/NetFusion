using Autofac.Core;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Takes an instance of the composite application and the built container registry
    /// and produces a nested dictionary structure representing the application that can
    /// be logged during host application initialization as JSON.
    /// </summary>
    internal class CompositeLog
    {
        private readonly CompositeApplication _application;
        private readonly IComponentRegistry _registry;

        public CompositeLog(CompositeApplication application, IComponentRegistry registry)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application), 
                "Composite Application to log cannot be null.");

            _registry = registry ?? throw new ArgumentNullException(nameof(registry),
                "Plug-in Registry to log cannot be null.");
        }

        public IDictionary<string, object> GetLog()
        {
            var log = new Dictionary<string, object> { };

            LogFoundPluginAssemblies(log);
            LogHostApp(log);
            LogAppComponentPlugins(log);
            LogCorePlugins(log);
            return log;
        }

        private void LogFoundPluginAssemblies(IDictionary<string, object> log)
        {
            log["Plugin:Assemblies"] = new Dictionary<string, object> {
                {"Host:Assembly", _application.AppHostPlugin.AssemblyName },
                {"Application:Assemblies", _application.AppComponentPlugins.Select(p => p.AssemblyName).ToArray() },
                {"Core:Assemblies", _application.CorePlugins.Select(p => p.AssemblyName).ToArray() }
            };
        }

        private void LogHostApp(IDictionary<string, object> log)
        {
            var hostLog = new Dictionary<string, object>();
            log["Plugin:Host"] = hostLog;

            LogPlugin(_application.AppHostPlugin, hostLog);
        }

        private void LogAppComponentPlugins(IDictionary<string, object> log)
        {
            log["Plugins:Application"] = _application.AppComponentPlugins.Select(plugin =>
            {
                var pluginLog = new Dictionary<string, object>();
                LogPlugin(plugin, pluginLog);
                return pluginLog;
            }).ToDictionary(p => p["Plugin:Id"].ToString());
        }

        private void LogCorePlugins(IDictionary<string, object> log)
        {
            log["Plugins:Core"] = _application.CorePlugins.Select(plugin =>
            {
                var pluginLog = new Dictionary<string, object>();
                LogPlugin(plugin, pluginLog);
                return pluginLog;
            }).ToDictionary(p => p["Plugin:Id"].ToString());
        }

        private void LogPlugin(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin:Name"] = plugin.Manifest.Name;
            log["Plugin:Id"] = plugin.Manifest.PluginId;
            log["Plugin:Assembly"] = plugin.Manifest.AssemblyName;
            log["Plugin:Description"] = plugin.Manifest.Description;
            log["Plugin:SourceUrl"] = plugin.Manifest.SourceUrl;
            log["Plugin:DocUrl"] = plugin.Manifest.DocUrl;

            LogPluginModules(plugin, log);
            LogPluginKnownTypeContracts(plugin, log);
            LogPluginKnownTypeDefinitions(plugin, log);
            LogPluginRegistrations(plugin, log);
        }

        private void LogPluginModules(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin:Modules"] = plugin.Modules.ToDictionary(
                m => m.GetType().FullName,
                pm =>
                {
                    var moduleLog = new Dictionary<string, object>();
                    pm.Log(moduleLog);
                    return moduleLog;
                });
        }

        private void LogPluginKnownTypeContracts(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin:KnownType:Contracts"] = plugin.PluginTypes
                .Where(pt => pt.IsKnownTypeContract)
                .Select(pt => pt.Type.FullName)
                .ToArray();
        }

        private void LogPluginKnownTypeDefinitions(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin:KnownType:Definitions"] = plugin.PluginTypes
                .Where(pt => pt.IsKnownTypeDefinition)
                .ToDictionary(
                    pt => pt.Type.FullName,
                    pt => pt.DiscoveredByPlugins.Select(dp => dp.Manifest.Name).ToArray()); 
        }

        private void LogPluginRegistrations(Plugin plugin, IDictionary<string, object> log)
        {
            log["Plugin:Type:Registrations"] = _registry.Registrations
                .Where(r => plugin.HasType(r.Activator.LimitType))
                .SelectMany(r => r.Services, (r, s) =>
                    new {
                        RegisteredType = r.Activator.LimitType.Name,
                        ServiceType = s.Description,
                        LifeTime = r.Lifetime.GetType().Name
                    }.ToDictionary()
                ).ToArray();
        }
    }
}
