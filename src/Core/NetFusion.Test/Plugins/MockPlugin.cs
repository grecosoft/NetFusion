using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System;
using System.Collections.Generic;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// Represents a mock plug-in that can be manually added by the host.
    /// Types can also be added to the plug-in to simulate plug-in types
    /// that would normally be loaded from the assembly at runtime.
    /// </summary>
    public abstract class MockPlugin : IPluginManifest,
        IPluginTypeAccessor
    {
        private List<Type> _pluginTypes = new List<Type>();
        public IEnumerable<Type> PluginTypes { get { return _pluginTypes; } }

        public MockPlugin()
        {
            PluginId = Guid.NewGuid().ToString();
            AssemblyName = $"Mock Assembly for Plug-in: {this.Name}";
            Name = GetType().Name + Guid.NewGuid();
        }

        public string PluginId { get; set; }
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }
        public string MachineName { get; set; }
        public string Name { get; set; }
        public string Description => "Mock Plug-in";
        public string SourceUrl => String.Empty;
        public string DocUrl => String.Empty;

        /// <summary>
        /// Adds one or more types to the plug-in.
        /// </summary>
        /// <param name="types">The type(s) to be added.</param>
        /// <returns>Reference to self.</returns>
        public IPluginTypeAccessor AddPluginType(params Type[] types)
        {
            if (types == null)throw new ArgumentNullException(nameof(types));

            _pluginTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Adds a type to the plug-in.
        /// </summary>
        /// <typeparam name="T">The type to be added.</typeparam>
        /// <returns>Reference to self.</returns>
        public IPluginTypeAccessor AddPluginType<T>()
        {
            _pluginTypes.Add(typeof(T));
            return this;
        }
    }
}
