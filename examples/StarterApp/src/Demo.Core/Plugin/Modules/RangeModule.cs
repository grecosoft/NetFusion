using Demo.Core.Plugin.Configs;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;

namespace Demo.Core.Plugin.Modules
{
    public class RangeModule : PluginModule
    {
        public override void Initialize() 
        {
            var config = Context.Plugin.GetConfig<ValidRangeConfig>();
            Context.BootstrapLogger.Add(LogLevel.Debug, config.ToIndentedJson());
        }
    }
}
