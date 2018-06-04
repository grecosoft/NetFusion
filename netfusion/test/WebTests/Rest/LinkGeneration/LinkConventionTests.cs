using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Test.Plugins;
using WebTests.Rest.ClientRequests;
using WebTests.Rest.LinkGeneration.Client;
using WebTests.Rest.LinkGeneration.Server;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.LinkGeneration
{
    public class LinkConventionTests
    {
        [Fact]
        public async Task ResourceMap_Convention_ResourceLinks()
        {
            // Arrange:
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<ConventionBasedResourceMap>();

            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };

            var serviceMock = new MockUnitTestService
            {
                ServerResources = new[] { mockResource }
            };

            // Act:
            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, serviceMock);

            var request = ApiRequest.Create("api/convention/links/resource", HttpMethod.Get);
            var resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;

            // Assert:
            resource.AssertLink("self", HttpMethod.Get, "/api/convention/links/self/10");
            resource.AssertLink("resource:create", HttpMethod.Post, "/api/convention/links/create");
            resource.AssertLink("resource:update", HttpMethod.Put, "/api/convention/links/update/10");
            resource.AssertLink("resource:delete", HttpMethod.Delete, "/api/convention/links/delete/10");
        }

        [Fact]
        public async Task ControllerAction_CanSpecifyReturnResource_UsingAttribute()
        {
            // Arrange:
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<ConventionBasedResourceMap>();

            var mockResource = new LinkedResource2
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };

            var serviceMock = new MockUnitTestService
            {
                ServerResources = new[] { mockResource }
            };

            // Act:
            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, serviceMock);

            var request = ApiRequest.Create("api/convention/links/resource2", HttpMethod.Get);
            var resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;

            // Assert:
            resource.Links.Should().HaveCount(1); 
            resource.AssertLink("self", HttpMethod.Get, "/api/convention/links/self/return/attribute/10");
        }
    }
}
