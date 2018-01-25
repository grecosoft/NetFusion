using System;
using NetFusion.Base.Exceptions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace NetFusion.Logging.Serilog.Enrichers
{
    /// <summary>
    /// Adds plug-in specific properties to the log event.
    /// </summary>
    public class NetFusionLogEnricher : ILogEventEnricher
    {
        private const string SOURCE_CONTEXT_PROP = "SourceContext";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {

            if (!logEvent.Properties.TryGetValue(SOURCE_CONTEXT_PROP, out LogEventPropertyValue pv))
            {
                AddDetailLogProperty(logEvent, propertyFactory);
                return;
            }

            ScalarValue sv = pv as ScalarValue;

            if (sv?.Value is string sourceContextType)
            {
                Plugin plugin = GetTypesPlugin(sourceContextType);
                if (plugin != null)
                {
                    AddPluginLogProperties(logEvent, propertyFactory, plugin);
                    return;
                }

                AddDetailLogProperty(logEvent, propertyFactory);
            }
        }

        private Plugin GetTypesPlugin(string contextType)
        {
            if (contextType == null) return null;

            var composite = (IComposite)AppContainer.Instance;
            return composite.GetPluginContainingFullTypeName(contextType);
        }

        private void AddPluginLogProperties(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, Plugin plugin)
        {
            IPluginManifest manifest = plugin.Manifest;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PluginId", manifest.PluginId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PluginName", manifest.Name));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PluginAssembly", manifest.AssemblyName));

            if (logEvent.Exception is NetFusionException ex)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Details", ex.Details.ToIndentedJson()));
            }
        }

        private void AddDetailLogProperty(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception is NetFusionException ex)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Details", ex.Details.ToIndentedJson()));
            }
        }
    }
}
