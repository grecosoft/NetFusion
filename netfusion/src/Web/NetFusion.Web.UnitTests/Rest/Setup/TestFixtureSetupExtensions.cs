using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Web.Rest.Server.Plugin;
using NetFusion.Web.UnitTests.Hosting;
using NetFusion.Web.UnitTests.Mocks;
using NetFusion.Web.UnitTests.Rest.ApiMetadata.Server;
using NetFusion.Web.UnitTests.Rest.ClientRequests.Server;
using NetFusion.Web.UnitTests.Rest.LinkGeneration.Server;

namespace NetFusion.Web.UnitTests.Rest.Setup;

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