using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Refactors
{
    public abstract class PluginDefinition : IPluginDefinition
    {
        // Properties that must be specified by derived plugins:
        public abstract string PluginId { get; }
        public abstract PluginDefinitionTypes PluginType { get; }
        public abstract string Name { get; }

        // Optional Properties that can be specified by derived plugins.
        public string Description { get; protected set; }
        public string SourceUrl { get; protected set; } 
        public string DocUrl { get; protected set; }
        
        // IPluginDefinition:
        public IEnumerable<IPluginModule> Modules => _modules;
        public IEnumerable<Type> Types { get; private set; }
        
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }

        
        private readonly List<IPluginModule> _modules = new List<IPluginModule>();
        private readonly List<IContainerConfig> _configs = new List<IContainerConfig>();

        protected void AddModule<TModule>() where TModule : IPluginModule, new()
        {
            _modules.Add(new TModule());
        }

        protected void AddConfig<TConfig>() where TConfig : IContainerConfig, new()
        {
            _configs.Add(new TConfig());
        }

        public IEnumerable<IContainerConfig> Configs => _configs;

        public void SetPluginMeta(string assemblyName, string assemblyVersion, 
            IEnumerable<Type> pluginTypes)
        {
            AssemblyName = assemblyName;
            AssemblyVersion = assemblyVersion;
            Types = pluginTypes;
        }
        
        public T GetConfig<T>() where T : IContainerConfig, new()
        {
            var config = _configs.FirstOrDefault(
                pc => pc.GetType() == typeof(T));

            if (config == null)
            {
                config = new T();
                _configs.Add(config);
            }

            return (T)config;
        }
    }
}