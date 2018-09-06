using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Web.Mvc.Composite.Models
{
    /// <summary>
    /// Model returned from the WebApi containing the root composite-structure
    /// of how the application was bootstrapped from assemblies containing plugins.
    /// </summary>
    public class CompositeStructure 
    {
        public string HostPluginAssembly { get; private set; }
        public string[] AppPluginAssemblies { get; private set; }
        public string[] CorePluginAssemblies { get; private set; }

        public PluginSummary HostPlugin { get; private set; }
        public PluginSummary[] CorePlugins { get; private set; }
        public PluginSummary[] ApplicationPlugins { get; private set; }

        // Creates a model from the generic composite log dictionary.
        public static CompositeStructure FromLog(IDictionary<string, object> compositeLog)
        {
            if (compositeLog == null)
            {
                throw new ArgumentNullException(nameof(compositeLog));
            }

            var loggedAssemblies = compositeLog["Plugin:Assemblies"] as IDictionary;
            var loggedCorePlugins = compositeLog["Plugins:Core"] as IDictionary;
            var loggedAppPlugins = compositeLog["Plugins:Application"] as IDictionary;

            return new CompositeStructure
            {
                // Assemblies:
                HostPluginAssembly = loggedAssemblies["Host:Assembly"] as string,
                AppPluginAssemblies = loggedAssemblies["Application:Assemblies"] as string[],
                CorePluginAssemblies = loggedAssemblies["Core:Assemblies"] as string[],

                // Plugins:
                CorePlugins = GetPlugins(loggedCorePlugins).ToArray(),
                ApplicationPlugins = GetPlugins(loggedAppPlugins).ToArray()
            };
        }

        private static IEnumerable<PluginSummary> GetPlugins(IEnumerable logEntries)
        {
            foreach (DictionaryEntry logEntry in logEntries)
            {
                var pluginProperties = logEntry.Value as IDictionary;
                yield return PluginSummary.FromLog(pluginProperties);
            }
        }
    }
}