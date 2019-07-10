using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Test.Hosting;
using NetFusion.Test.Plugins;
using WebTests.Rest.ClientRequests;
using WebTests.Rest.ClientRequests.Server;
using WebTests.Rest.LinkGeneration.Server;

namespace WebTests.Rest.Setup
{
    public static class TestFixtureSetupExtensions
    {
        public static WebServerConfig ArrangeWithDefaults(this WebHostFixture fixture, LinkedResource mockResource)
        {
            return fixture.WithServices(services =>
                {
                    var serviceMock = new MockUnitTestService
                    {
                        ServerResources = new[] {mockResource}
                    };

                    services.AddSingleton<IMockedService>(serviceMock);
                })
                .ComposedFrom(compose =>
                {
                    compose.AddRest();

                    var hostPlugin = new MockHostPlugin();
                    hostPlugin.AddPluginType<LinkedResourceMap>();

                    compose.AddPlugin(hostPlugin);
                });
        }
        
        public static WebServerConfig ArrangeWithDefaults(this WebHostFixture fixture, IMockedService service = null)
        {
            return fixture.WithServices(services =>
                {
                    var serviceMock = new MockUnitTestService
                    {
                        ServerResources = new object[]{}
                    };

                    services.AddSingleton(service ?? serviceMock);
                })
                .ComposedFrom(compose =>
                {
                    compose.AddRest();

                    var hostPlugin = new MockHostPlugin();
                    hostPlugin.AddPluginType<LinkedResourceMap>();
                    hostPlugin.AddPluginType<CustomerResourceMap>();

                    compose.AddPlugin(hostPlugin);
                });
        }
    }
}