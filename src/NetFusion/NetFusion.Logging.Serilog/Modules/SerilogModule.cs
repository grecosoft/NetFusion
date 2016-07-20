using Autofac;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Serilog.Core;
using Serilog;

namespace NetFusion.Logging.Serilog.Modules
{
    public class SerilogModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.Register(c => 
                new SerilogPluginLogger(c.Resolve<ILogger>())
            )
            .As<IContainerLogger>()
            .ExternallyOwned();
        }
    }
}
