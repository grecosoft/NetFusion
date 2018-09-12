using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;

namespace Demo.Infra
{
    public class RangeModule: PluginModule
    {
        private ValidRangeConfig Config { get; set; }

        public override void Initialize()
        {
            Config = Context.Plugin.GetConfig<ValidRangeConfig>();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["min-value"] = Config.MinValue;
            moduleLog["max-value"] = Config.MaxValue;
            moduleLog["difference"] = Config.MaxValue - Config.MinValue;
        }
    }
}
