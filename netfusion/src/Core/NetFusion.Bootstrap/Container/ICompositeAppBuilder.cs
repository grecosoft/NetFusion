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

        T GetConfig<T>() where T : IContainerConfig;
    }
}