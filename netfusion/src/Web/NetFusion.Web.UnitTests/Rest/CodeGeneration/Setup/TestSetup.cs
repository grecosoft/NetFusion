using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Web.Rest.CodeGen.Plugin;
using NetFusion.Web.UnitTests.Rest.CodeGeneration.Server;

namespace NetFusion.Web.UnitTests.Rest.CodeGeneration.Setup;

/// <summary>
/// Common test configurations.
/// </summary>
public static class TestSetup
{
    /// <summary>
    /// Adds to the composite container the Code-Generation Plugin and a
    /// Host Plugin containing two models for which code will be generated.
    /// </summary>
    /// <param name="container"></param>
    public static void WithDefaults(CompositeContainer container)
    {
        var hostPlugin = new MockHostPlugin();
        hostPlugin.AddPluginType<ApiModelOne>();
        hostPlugin.AddPluginType<ApiModelTwo>();

        container.RegisterPlugins(hostPlugin);
        container.RegisterPlugin<CodeGenPlugin>();
    }
}