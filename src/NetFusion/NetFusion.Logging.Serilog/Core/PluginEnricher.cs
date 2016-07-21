using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using Serilog.Core;
using Serilog.Events;
using System;

namespace NetFusion.Logging.Serilog.Core
{
    /// <summary>
    /// Adds plug-in specific properties to the log event.
    /// </summary>
    public class PluginEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            LogEventPropertyValue pv = null;

            if (!logEvent.Properties.TryGetValue(SerilogManifest.ContextPropName, out pv)) return;

            ScalarValue sv = pv as ScalarValue;
            string sourceContextType = sv?.Value as string;

            if (sourceContextType != null)
            {
                Plugin plugin = GetTypesPlugin(Type.GetType(sourceContextType, false));
                if (plugin != null)
                {
                    AddPluginLogProperties(logEvent, propertyFactory, plugin);
                }
            }

            logEvent.RemovePropertyIfPresent(SerilogManifest.ContextPropName);
        }

        private Plugin GetTypesPlugin(Type contextType)
        {
            if (contextType == null) return null;

            var composite = (IComposite)AppContainer.Instance;
            return composite.GetPluginForType(contextType);
        }

        private void AddPluginLogProperties(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, Plugin plugin)
        {
            IPluginManifest manifest = plugin.Manifest;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PluginId", manifest.PluginId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PluginName", manifest.Name));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PluginAssembly", manifest.AssemblyName));
        }
    }
}
