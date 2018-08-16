using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;

namespace Demo.Infra
{
    public class RangeModule: PluginModule
    {
        public override void Initialize()
        {
            var config = Context.Plugin.GetConfig<ValidRangeConfig>();
            Context.Logger.LogDebug(config.ToIndentedJson());
        }
    }
}
