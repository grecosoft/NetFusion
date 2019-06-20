using Demo.App.Plugin.Modules;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Plugin
{
    public class AppPlugin : PluginBase
    {
        public override string PluginId => "C7D17C4B-2604-4310-97B7-7208AD8027F3";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Application Services Component";

        public AppPlugin()
        {
            AddModule<ServiceModule>();
            AddModule<LifeCycleModule>();
            Description = "Plugin component containing the Microservice's application services.";
        }   
    }
}
