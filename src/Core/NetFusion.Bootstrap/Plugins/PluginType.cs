using NetFusion.Base.Plugins;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Additional information associated with the .NET type.
    /// </summary>
    public class PluginType
    {
        public PluginType(Plugin plugin, Type type, string assemblyName)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

            IsKnownTypeContract = type.IsDerivedAbstractType<IKnownPluginType>();
            IsKnownTypeDefinition = type.IsConcreteTypeDerivedFrom<IKnownPluginType>();
        }

        /// <summary>
        /// The plug-in containing the type.
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        /// The underlying .NET type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Indicates that the type is abstract and implements the IKnownPluginType interface.
        /// </summary>
        public bool IsKnownTypeContract { get; set; }

        /// <summary>
        /// Indicates that the type implements the IKnownPluginType interface and 
        /// is a concrete instance.
        /// </summary>
        public bool IsKnownTypeDefinition { get; }

        /// <summary>
        /// The name of the .NET assembly containing the type.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// If a known type, the plug-ins that discovered the type.
        /// </summary>
        public IEnumerable<Plugin> DiscoveredByPlugins { get; set; }
    }
}
