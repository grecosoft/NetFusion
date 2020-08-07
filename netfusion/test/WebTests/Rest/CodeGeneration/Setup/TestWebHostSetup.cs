using NetFusion.Rest.CodeGen.Plugin;
using NetFusion.Test.Plugins;
using WebTests.Hosting;
using WebTests.Rest.CodeGeneration.Server;

namespace WebTests.Rest.CodeGeneration.Setup
{
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
            return fixture.WithServices(services =>
                {

                })
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
}