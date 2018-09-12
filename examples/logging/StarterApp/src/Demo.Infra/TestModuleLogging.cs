using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Infra
{
    public class TestModuleLogging : PluginModule
    {
        public override void Initialize()
        {
            Context.Logger.LogWarning("Warning from Plugin Module");

            var namedLogger = Context.LoggerFactory
                .CreateLogger("NetFusion Examples");

            namedLogger.LogError("Just an example.");
        }
    }
}
