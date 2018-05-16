using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Web.Mvc.Composite.Models
{
    /// <summary>
    /// Model containing the details of plugin created from the composite log.
    /// </summary>
    public class PluginDetails 
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Assembly { get; private set; }
        public string Description { get; private set; }
        public string SourceUrl { get; private set; }
        public string DocUrl { get; private set; }

        public string[] KnownTypeContracts { get; private set; }
        public KnownTypeDefinition[] KnowTypeDefinitions { get; private set; }
        public PluginModule[] Modules { get; private set; }
        public RegisteredService[] Registrations { get; private set; }

        public static PluginDetails FromLog(IDictionary pluginLog)
        {
            if (pluginLog == null)
            {
                throw new ArgumentNullException(nameof(pluginLog));
            }

            var knownTypeContracts = pluginLog["Plugin:KnownType:Contracts"] as string[];
            var knowTypeDefinitions = pluginLog["Plugin:KnownType:Definitions"] as IDictionary;
            var pluginModules = pluginLog["Plugin:Modules"] as IDictionary;
            var pluginRegistrations = pluginLog["Plugin:Type:Registrations"] as ICollection;

            var pluginResource = new PluginDetails
            {
                Id = pluginLog["Plugin:Id"] as string,
                Name = pluginLog["Plugin:Name"] as string,
                Assembly = pluginLog["Plugin:Assembly"] as string,
                Description = pluginLog["Plugin:Description"] as string,
                SourceUrl = pluginLog["Plugin:SourceUrl"] as string,
                DocUrl = pluginLog["Plugin:DocUrl"] as string,

                // Defined contracts and implementations.
                KnownTypeContracts = knownTypeContracts,
                KnowTypeDefinitions = GetKnownTypeDefinitions(knowTypeDefinitions).ToArray(),

                // Plugin contained modules and registered plugin types with the DI container 
                Modules = GetPluginModules(pluginModules).ToArray(),
                Registrations = GetPluginRegistrations(pluginRegistrations).ToArray()
            };

            return pluginResource;
        }

        private static IEnumerable<KnownTypeDefinition> GetKnownTypeDefinitions(IDictionary concreteKnownTypes)
        {
            foreach (var knownType in concreteKnownTypes.Keys)
            {
                var discoveredByPluginNames = concreteKnownTypes[knownType] as string[];
                yield return new KnownTypeDefinition
                {
                    DefinitionTypeName = knownType.ToString(),
                    DiscoveringPlugins = discoveredByPluginNames
                };
            }
        }

        private static IEnumerable<PluginModule> GetPluginModules(IDictionary pluginModules)
        {
            foreach (var pluginModule in pluginModules.Keys)
            {
                yield return new PluginModule
                {
                    Name = pluginModule.ToString(),
                    Log = pluginModules[pluginModule] as IDictionary
                };
            }
        }

        private  static IEnumerable<RegisteredService> GetPluginRegistrations(ICollection registrations)
        {
            foreach (IDictionary registration in registrations)
            {
                yield return new RegisteredService
                {
                    RegisteredType = registration["RegisteredType"] as string,
                    ServiceType = registration["ServiceType"] as string,
                    LifeTime = registration["LifeTime"] as string
                };
            }
        }
    }
}