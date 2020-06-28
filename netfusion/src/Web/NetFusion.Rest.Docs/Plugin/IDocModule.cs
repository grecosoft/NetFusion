using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Plugin.Configs;

namespace NetFusion.Rest.Docs.Plugin
{
    public interface IDocModule : IPluginModuleService
    {
        RestDocConfig RestDocConfig { get; }
    }
}