using NetFusion.Bootstrap.Plugins;
using Demo.App.Plugin.Modules;

namespace Demo.App.Plugin
{
    public class AppPlugin : PluginBase
    {
        public override string PluginId => "eb3973f3-fc67-480b-b827-49f4c873fbbd";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Application Services Component";

        public AppPlugin()
        {
            AddModule<ServiceModule>();

            Description = "Plugin component containing the Microservice's application services.";
        }   
    }
}