using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    public interface ICompositeContainerBuilder
    {
        ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new();
        IBuiltContainer Build();
        T GetConfig<T>() where T : IPluginConfig;
    }
}