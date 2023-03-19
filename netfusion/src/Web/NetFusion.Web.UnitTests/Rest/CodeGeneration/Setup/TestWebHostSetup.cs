using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Web.Rest.CodeGen.Plugin;
using NetFusion.Web.UnitTests.Hosting;
using NetFusion.Web.UnitTests.Rest.CodeGeneration.Server;

namespace NetFusion.Web.UnitTests.Rest.CodeGeneration.Setup;

/// <summary>
/// Configured TestServer bootstrapped with NetFusion.
/// </summary>
public static class TestWebHostSetup
{
    /// <summary>
    /// Configures a TestServer bootstrapped with the Code-Generation plugin.
    /// Also adds two models for which TypeScript will be generated.  The
    /// middleware component that exposes a REST Api to query the code is
    /// added to the application-builder.
    /// </summary>
    /// <param name="fixture">The Web Host fixture to arrange.</param>
    /// <returns>Created Web Server configuration to be acted on.</returns>
    public static WebServerConfig ArrangeForRestCodeGen(this WebHostFixture fixture)
    {
        return fixture
            .UsingAppSerivces(appBuilder => appBuilder.UseRestCodeGen())
            .ComposedFrom(compose =>
            {
                compose.AddRestCodeGen();

                var hostPlugin = new MockHostPlugin();
                hostPlugin.AddPluginType<ApiModelOne>();
                hostPlugin.AddPluginType<ApiModelTwo>();

                compose.AddPlugin(hostPlugin);
            });
    }
}