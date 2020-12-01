using Demo.Core.Plugin.Configs;
using Microsoft.Extensions.Logging;
using NetFusion.Base;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;

namespace Demo.Core.Plugin.Modules
{
    public class RangeModule : PluginModule
    {
        public override void Initialize() 
        {
            var config = Context.Plugin.GetConfig<ValidRangeConfig>();
            NfExtensions.Logger.Log<RangeModule>(LogLevel.Debug, config.ToIndentedJson());
        }
    }
}
