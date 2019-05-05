using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Plugins
{
    using NetFusion.Bootstrap.Exceptions;

    /// <summary>
    /// Base class providing a default implementation of the IPlugin interface.
    /// Each assembly defining a plugin should define a derived type containing
    /// metadata, configurations, and modules for which the plugin is comprised.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        // Properties that must be specified by derived plugins:
        public abstract string PluginId { get; }
        public abstract PluginTypes PluginType { get; }
        public abstract string Name { get; }

        // Optional Properties that can be specified by derived plugins.
        public string Description { get; protected set; }
        public string SourceUrl { get; protected set; } 
        public string DocUrl { get; protected set; }
        
        // Plugin Parts:
        public IEnumerable<IPluginConfig> Configs => _configs;
        public IEnumerable<IPluginModule> Modules => _modules;
        public IEnumerable<Type> Types { get; protected set; } = new List<Type>();
        
        // Assembly Metadata.
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }

        private readonly List<IPluginConfig> _configs = new List<IPluginConfig>();
        private readonly List<IPluginModule> _modules = new List<IPluginModule>();

        /// <summary>
        /// Adds a module to the plugin.
        /// </summary>
        /// <typeparam name="TModule">The module type.</typeparam>
        protected void AddModule<TModule>() where TModule : IPluginModule, new()
        {
            _modules.Add(new TModule());
        }

        /// <summary>
        /// Adds a configuration to the plugin.
        /// </summary>
        /// <typeparam name="TConfig">The configuration type.</typeparam>
        protected void AddConfig<TConfig>() where TConfig : IPluginConfig, new()
        {
            _configs.Add(new TConfig());
        }

        public void SetPluginMeta(string assemblyName, string assemblyVersion, 
            IEnumerable<Type> pluginTypes)
        {
            AssemblyName = assemblyName;
            AssemblyVersion = assemblyVersion;
            Types = pluginTypes;
        }

        public bool HasType(Type pluginType)
        {
            if (pluginType == null) throw new ArgumentNullException(nameof(pluginType));
            return Types.Any(pt => pt == pluginType);
        }
        
        public T GetConfig<T>() where T : IPluginConfig
        {
            var config = _configs.FirstOrDefault(
                pc => pc.GetType() == typeof(T));

            if (config == null)
            {
                throw new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} is not registered.");
            }

            return (T)config;
        }
    }
}