using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface containing the plugins and modules
    /// used to build the composite-application.
    /// </summary>
    public interface ICompositeAppBuilder
    {
        IPlugin HostPlugin { get; }
        IPlugin[] AppPlugins { get; }
        IPlugin[] CorePlugins { get; }
        
        IPlugin[] AllPlugins { get; }
        IPluginModule[] AllModules { get; }
        
        CompositeAppLog CompositeLog { get; }

        IConfiguration Configuration { get; }
        IEnumerable<Type> GetPluginTypes(params PluginTypes[] pluginTypes);
        T GetConfig<T>() where T : IContainerConfig;
    }
}