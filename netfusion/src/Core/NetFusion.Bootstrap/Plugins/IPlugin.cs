using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Plugins
{
    public interface IPlugin
    {
        string PluginId { get; }
        PluginTypes PluginType { get; }    
        string Name { get; }

        string AssemblyName { get; set; }
        string AssemblyVersion { get; set; }
        
        string Description { get; }
        string SourceUrl { get; }
        string DocUrl { get; }
        
        IEnumerable<IPluginModule> Modules { get; }
        IEnumerable<IPluginConfig> Configs { get; }
        IEnumerable<Type> Types { get; }

        void SetPluginMeta(string assemblyName, string assemblyVersion,
            IEnumerable<Type> pluginTypes);

        bool HasType(Type pluginType);

        T GetConfig<T>() where T : IPluginConfig, new();
    }
}