using NetFusion.Core.Bootstrap.Plugins;
using Solution.Context.App.Plugin.Modules;

namespace Solution.Context.App.Plugin;

public class AppPlugin : PluginBase
{
    public override string PluginId => "[nf:app-id]";
    public override PluginTypes PluginType => PluginTypes.AppPlugin;
    public override string Name => "Application Services Component";

    public AppPlugin()
    {
        AddModule<ServiceModule>();

        Description = "Plugin component containing the Microservice's application services.";
    }   
}