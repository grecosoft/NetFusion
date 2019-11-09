using Demo.Core.Plugin.Configs;
using Demo.Core.Plugin.Modules;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Core.Plugin
{
    public class CorePlugin : PluginBase
    {
        public override string PluginId => "281a5f10-b735-448c-81c4-af71858dae9f3";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "Core Component";
        
        public CorePlugin()
        {
            AddConfig<ValidRangeConfig>();
            AddModule<RangeModule>();

            AddModule<CalculatorModule>();
            AddModule<NumberGeneratorModule>();

            Description = "Plugin component containing the Microservice's domain model.";
        }
    }
}    
