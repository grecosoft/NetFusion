using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Test.Plugins;

namespace WebTests.Rest.DocGeneration.Setup
{
    public class PluginSetup
    {
        public static void WithDefaults(CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();

            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<DocsPlugin>();
        }
    }
}