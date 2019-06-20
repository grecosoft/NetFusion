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
            var config = this.Context.Plugin.GetConfig<ValidRangeConfig>();
            Context.Logger.LogDebug(config.ToIndentedJson());
        }
    }
}
