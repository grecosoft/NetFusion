using System.Net.Http;
using System.Threading.Tasks;
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
    /// <summary>
    /// Tests the applying of one resource type's links to another resource type.
    /// This is used for when there are various view model resources providing
    /// different views of a base resource.  Since the view model resources are
    /// just a different view of the base underlying resource, the links defined
    /// for the base resource can be applied.  This reduces code duplication so
    /// the same links don't have to be defined multiple times.
    /// </summary>
    public class LinkCopyGenerationTests
    {
        [Fact]
        public async Task ExistingResourceMetadata_CanBeApplied_ToAnotherResourceType()
        {
            // Arrange:
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<LinkedResourceMap>();

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

            var request = ApiRequest.Create("api/linked/resource/view", HttpMethod.Get);

            var resource = (await client.SendAsync<LinkedViewResourceModel>(request)).Content;

            // Assert:
            // --------------------------------------------------
            // Required Route Parameters:
            resource.AssertLink("scenario-1", HttpMethod.Get, "/api/linked/resource/scenario-1/10");
            resource.AssertLink("scenario-2", HttpMethod.Get, "/api/linked/resource/scenario-2/10/param-one/value-2");

            // Optional Route Parameter with supplied value:
            resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one/300");

            // Optional Route Parameter with no supplied value:
            mockResource.Value3 = null;
            resource = (await client.SendAsync<LinkedViewResourceModel>(request)).Content;
            resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one");

            // Multiple Optional Parameters with supplied values.
            mockResource.Value3 = 600;
            mockResource.Value2 = "value-2";
            resource = (await client.SendAsync<LinkedViewResourceModel>(request)).Content;
            resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one/600/value-2");

            // Multiple optional Parameters with no supplied value.
            mockResource.Value3 = null;
            mockResource.Value2 = null;
            resource = (await client.SendAsync<LinkedViewResourceModel>(request)).Content;
            resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one");

            // No route parameters with single parameter populated from posted data.
            resource = (await client.SendAsync<LinkedViewResourceModel>(request)).Content;
            resource.AssertLink("scenario-5", HttpMethod.Post, "/api/linked/resource/scenario-5/create");

            // Single route parameter with additional class based parameter populated from posted data.
            resource = (await client.SendAsync<LinkedViewResourceModel>(request)).Content;
            resource.AssertLink("scenario-6", HttpMethod.Put, "/api/linked/resource/scenario-6/10/update");
        }
    }
}
