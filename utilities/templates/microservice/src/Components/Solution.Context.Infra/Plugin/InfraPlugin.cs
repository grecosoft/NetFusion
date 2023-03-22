using NetFusion.Core.Bootstrap.Plugins;
using Solution.Context.Infra.Plugin.Modules;

namespace Solution.Context.Infra.Plugin;

public class InfraPlugin : PluginBase
{
    public override string PluginId => "[nf:infra-id]";
    public override PluginTypes PluginType => PluginTypes.AppPlugin;
    public override string Name => "Infrastructure Application Component";

    public InfraPlugin() {
        AddModule<RepositoryModule>();

        Description = "Plugin component containing the application infrastructure.";
    }
}