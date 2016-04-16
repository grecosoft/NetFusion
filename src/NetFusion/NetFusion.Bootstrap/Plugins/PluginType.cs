using NetFusion.Common.Extensions;
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
            this.Plugin = plugin;
            this.Type = type;
            this.AssemblyName = assemblyName;
            this.IsKnownType = type.IsDerivedFrom<IKnownPluginType>();
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
        /// Indicates that the type implements the IKnownPluginType
        /// interface.
        /// </summary>
        public bool IsKnownType { get; }

        /// <summary>
        /// The name of the .NET assembly where the type was found.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// The plug-ins that discovered the type.
        /// </summary>
        public IEnumerable<Plugin> DiscoveredByPlugins { get; set; }
    }
}
