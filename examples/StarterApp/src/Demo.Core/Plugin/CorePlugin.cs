using Demo.Core.Plugin.Configs;
using Demo.Core.Plugin.Modules;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Core.Plugin
{
    public class CorePlugin : PluginBase
    {
        public override string PluginId => "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "Core Plugin Component";

        public CorePlugin()
        {
            AddModule<CalculatorModule>();
            AddModule<NumberGeneratorModule>();
            
            AddConfig<ValidRangeConfig>();
            AddModule<RangeModule>();

            Description = "Plugin component containing core implementations.";
        }
    }
}
