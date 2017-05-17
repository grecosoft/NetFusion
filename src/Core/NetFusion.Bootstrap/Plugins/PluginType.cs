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
        /// Indicates that the type implements the IKnownPluginType interface.
        /// </summary>
        public bool IsKnownType { get; }

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
