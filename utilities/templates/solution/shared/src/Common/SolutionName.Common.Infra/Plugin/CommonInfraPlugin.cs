using NetFusion.Core.Bootstrap.Plugins;
using SolutionName.Common.Infra.Plugin.Modules;

namespace SolutionName.Common.Infra.Plugin;

public class CommonInfraPlugin : PluginBase
{
    public override string PluginId => "[nf:infra-id]";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "SolutionName Common Service Components";

    public CommonInfraPlugin() {
        AddModule<ServiceModule>();

        Description = "Plugin component containing service infrastructure components.";
    }
}