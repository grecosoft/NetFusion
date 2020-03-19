using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Server.Plugin;
using WebTests.Hosting;
using NetFusion.Test.Plugins;
using CustomerResourceMap = WebTests.Rest.ClientRequests.Server.CustomerResourceMap;
using IMockedService = WebTests.Rest.ClientRequests.IMockedService;
using LinkedResource = WebTests.Rest.LinkGeneration.Server.LinkedResource;
using LinkedResourceMap = WebTests.Rest.LinkGeneration.Server.LinkedResourceMap;
using MockUnitTestService = WebTests.Rest.ClientRequests.MockUnitTestService;

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