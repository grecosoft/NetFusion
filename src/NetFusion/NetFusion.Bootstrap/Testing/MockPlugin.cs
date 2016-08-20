using NetFusion.Bootstrap.Manifests;
using NetFusion.Common;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Testing
{
    /// <summary>
    /// Represents a mock plug-in that can be manually added by the host.
    /// Types can also be added to the plug-in to simulate plug-in types
    /// that would normally be loaded from the assembly at runtime.
    /// </summary>
    public abstract class MockPlugin : IPluginManifest
    {
        private List<Type> _pluginTypes = new List<System.Type>();
        public IEnumerable<Type> PluginTypes { get { return _pluginTypes; } }

        public MockPlugin()
        {
            this.PluginId = Guid.NewGuid().ToString();
            this.AssemblyName = $"Mock Assembly for Plug-in: {this.Name}";
            this.Name = this.GetType().Name;
        }

        public string PluginId { get; set; }
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }
        public string MachineName { get; set; }
        public string Name { get; set; }
        public string Description => "Mock Plug-in";

        /// <summary>
        /// Adds one or more types to the plug-in.
        /// </summary>
        /// <param name="types">The type(s) to be added.</param>
        /// <returns>Reference to self.</returns>
        public MockPlugin AddPluginType(params Type[] types)
        {
            Check.NotNull(types, nameof(types), "types not specified");

            _pluginTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Adds a type to the plug-in.
        /// </summary>
        /// <typeparam name="T">The type to be added.</typeparam>
        /// <returns>Reference to self.</returns>
        public MockPlugin AddPluginType<T>()
        {
            _pluginTypes.Add(typeof(T));
            return this;
        }
    }
}
