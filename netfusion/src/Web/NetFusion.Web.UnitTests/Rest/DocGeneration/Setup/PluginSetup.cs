using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Web.Rest.Docs.Plugin;

namespace NetFusion.Web.UnitTests.Rest.DocGeneration.Setup;

public class PluginSetup
{
    public static void WithDefaults(CompositeContainer container)
    {
        var hostPlugin = new MockHostPlugin();

        container.RegisterPlugins(hostPlugin);
        container.RegisterPlugin<DocsPlugin>();
    }
}