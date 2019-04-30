using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    public interface ICompositeContainerBuilder
    {
        ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new();
        ICompositeContainerBuilder InitConfig<T>(Action<T> configure) where T : IPluginConfig;
        
        IBuiltContainer Build();
        T GetPluginConfig<T>() where T : IPluginConfig;
    }
}