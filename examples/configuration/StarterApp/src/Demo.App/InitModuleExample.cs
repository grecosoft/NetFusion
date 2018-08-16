using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App
{
    public class InitModuleExample: PluginModule
    {
        public override void Initialize()
        {
            Context.Logger.LogDebug("--Initialize Called--");   
            Context.Logger.LogDebug(Context.AppHost.Manifest.Name);
            Context.Logger.LogDebug(Context.Plugin.Manifest.Name); 
        }

        public override void Configure()
        {
            Context.Logger.LogDebug("--Configure Called--");
        }

        public override void StartModule(System.IServiceProvider services)
        {
            Context.Logger.LogDebug("--StartModule Called--");
        }

        public override void RunModule(System.IServiceProvider services)
        {
            Context.Logger.LogDebug("--RunModule Called--");
        }

        public override void StopModule(System.IServiceProvider services)
        {
            Context.Logger.LogDebug("--StopModule Called--");
        }
    }
}
