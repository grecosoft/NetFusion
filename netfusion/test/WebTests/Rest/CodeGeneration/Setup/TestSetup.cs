using NetFusion.Bootstrap.Container;
using NetFusion.Rest.CodeGen.Plugin;
using NetFusion.Test.Plugins;
using WebTests.Rest.CodeGeneration.Server;

namespace WebTests.Rest.CodeGeneration.Setup
{
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
}