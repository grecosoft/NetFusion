using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface containing the information from which the composite-application is built.
    /// </summary>
    public interface ICompositeAppBuilder
    {
        // Categorized Plugins:
        IPlugin HostPlugin { get; }
        IPlugin[] AppPlugins { get; }
        IPlugin[] CorePlugins { get; }
        
        // All Plugins and Modules.
        IPlugin[] AllPlugins { get; }
        IPluginModule[] AllModules { get; }

        IEnumerable<Type> GetPluginTypes(params PluginTypes[] pluginTypes);

        // Configurations:
        IConfiguration Configuration { get; }
        void AddContainerConfig(IContainerConfig containerConfig);
        T GetContainerConfig<T>() where T : IContainerConfig;
        
        CompositeAppLog CompositeLog { get; }
        
        IBootstrapLogger BootstrapLogger { get; }
    }
}