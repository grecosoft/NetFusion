using NetFusion.Bootstrap.Container;

namespace NetFusion.Bootstrap.Refactors
{
    public interface IComposeAppBuilder
    {
        IComposeAppBuilder AddPlugin<TPlugin>() where TPlugin : IPluginDefinition, new();
        IBuiltContainer Build();
        T GetConfig<T>() where T : IContainerConfig;
    }
}