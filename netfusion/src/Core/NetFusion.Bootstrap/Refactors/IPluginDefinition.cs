using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Refactors
{
    public interface IPluginDefinition
    {
        string PluginId { get; }
        PluginDefinitionTypes PluginType { get; }    
        string Name { get; }

        string AssemblyName { get; set; }
        string AssemblyVersion { get; set; }
        
        string Description { get; }
        string SourceUrl { get; }
        string DocUrl { get; }
        
        IEnumerable<IPluginModule> Modules { get; }
        IEnumerable<IContainerConfig> Configs { get; }
        IEnumerable<Type> Types { get; }

        void SetPluginMeta(string assemblyName, string assemblyVersion,
            IEnumerable<Type> pluginTypes);

        T GetConfig<T>() where T : IContainerConfig, new();
    }
}