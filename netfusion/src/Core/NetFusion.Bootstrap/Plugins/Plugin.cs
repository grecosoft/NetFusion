using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Plugins
{
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
        
        // IPluginDefinition:
        public IEnumerable<IPluginModule> Modules => _modules;
        public IEnumerable<Type> Types { get; private set; }
        
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }

        
        private readonly List<IPluginModule> _modules = new List<IPluginModule>();
        private readonly List<IPluginConfig> _configs = new List<IPluginConfig>();

        protected void AddModule<TModule>() where TModule : IPluginModule, new()
        {
            _modules.Add(new TModule());
        }

        protected void AddConfig<TConfig>() where TConfig : IPluginConfig, new()
        {
            _configs.Add(new TConfig());
        }

        public IEnumerable<IPluginConfig> Configs => _configs;

        public void SetPluginMeta(string assemblyName, string assemblyVersion, 
            IEnumerable<Type> pluginTypes)
        {
            AssemblyName = assemblyName;
            AssemblyVersion = assemblyVersion;
            Types = pluginTypes;
        }

        public bool HasType(Type pluginType)
        {
            return Types.Any(pt => pt == pluginType);
        }
        
        public T GetConfig<T>() where T : IPluginConfig, new()
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