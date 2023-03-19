using Examples.nfTopic.Infra.Plugin.Modules;
using NetFusion.Core.Bootstrap.Plugins;

namespace Examples.nfTopic.Infra.Plugin;

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