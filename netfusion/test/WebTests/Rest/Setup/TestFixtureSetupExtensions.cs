using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Server.Plugin;
using WebTests.Hosting;
using NetFusion.Test.Plugins;
using WebTests.Mocks;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.ClientRequests.Server;
using WebTests.Rest.LinkGeneration.Server;

namespace WebTests.Rest.Setup
{
    public static class TestFixtureSetupExtensions
    {
        public static WebServerConfig ArrangeWithDefaults(this WebHostFixture fixture, params object[] mockResources)
        {
            return fixture.WithServices(services =>
                {
                    var serviceMock = new MockUnitTestService
                    {
                        ServerResources = mockResources
                    };
                    
                    services.AddSingleton<IMockedService>(serviceMock);
                })
                .ComposedFrom(compose =>
                {
                    compose.AddRest();

                    var hostPlugin = new MockHostPlugin();
                    hostPlugin.AddPluginType<ResourceMap>();
                    hostPlugin.AddPluginType<MetaResourceMap>();

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
                    hostPlugin.AddPluginType<ResourceMap>();
                    hostPlugin.AddPluginType<CustomerResourceMap>();

                    compose.AddPlugin(hostPlugin);
                });
        }
    }
}