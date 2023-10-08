using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

public interface ICompositeContainer
{ 
    bool IsComposed { get; } 
    ICompositeAppBuilder AppBuilder { get; }
    T GetPluginConfig<T>() where T : IPluginConfig;
    void RegisterPlugin<T>() where T : IPlugin, new();
    void RegisterPlugins(params IPlugin[] plugins);
}