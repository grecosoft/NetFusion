using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    public interface IComposeAppBuilder
    {
        IComposeAppBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new();
        IBuiltContainer Build();
        T GetConfig<T>() where T : IPluginConfig;
    }
}