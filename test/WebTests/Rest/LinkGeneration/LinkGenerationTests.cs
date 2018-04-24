using InfrastructureTests.Web.Rest.ClientRequests;
using InfrastructureTests.Web.Rest.LinkGeneration.Client;
using InfrastructureTests.Web.Rest.LinkGeneration.Server;
using InfrastructureTests.Web.Rest.Setup;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Test.Plugins;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace InfrastructureTests.Web.Rest.LinkGeneration
{
    /// <summary>
    /// Unit-tests 
    /// </summary>
    public class LinkGenerationTests
    {        
        /// <summary>
        /// The following tests the selection of resource links using type safe expressions to specify
        /// the controller and action method to call for a given resource URL.  This is the approach that
        /// should be use most often.  This allows resource mappings to specify the intent of the link and
        /// allows the MVC routing to determine the URL based on the current configuration.
        /// 
        /// Scenarios:
        ///     Scenario 1: Controller Action with single required route parameter.
        ///     
        ///     Scenario 2: Controller Action with two required route parameters.
        ///     
        ///     Scenario 3: Controller Action with one required and one optional route parameter.  Testing
        ///         when resource property has value for optional parameter and when the resource property
        ///         does not have a specific value.
        ///         
        ///     Scenario 4: Controller Action with multiple optional parameters.
        ///     
        ///     Scenario 5: Controller Action with no route parameter with one parameter sourced from posted data.
        ///         
        ///     Scenario 6: Controller Action with single route sourced parameter and additional parameter
        ///         sourced from the posted data.
        ///     
        /// </summary>
        /// <example>
        /// 
        ///     .Map<LinkedResource>()
        ///         .LinkMeta<LinkedResourceController>(
        ///             meta => meta.Url("scenario-1", (c, r) => c.GetById(r.IdValue)))
        ///
        ///         .LinkMeta<LinkedResourceController>(
        ///             meta => meta.Url("scenario-2", (c, r) => c.GetByIdAndRequiredParam(r.IdValue, r.Value2)));
        ///             
        ///         ...
        ///             
        /// </example>
        [Fact]
        public async Task CanGenerateUrl_FromControllerActionExpression()
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

            var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);

			var resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;

            // Assert:
            // --------------------------------------------------
            // Required Route Parameters:
            resource.AssertLink("scenario-1", HttpMethod.Get, "/api/linked/resource/scenario-1/10");
            resource.AssertLink("scenario-2", HttpMethod.Get, "/api/linked/resource/scenario-2/10/param-one/value-2");

            // Optional Route Parameter with supplied value:
            resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one/300");

            // Optional Route Parameter with no supplied value:
            mockResource.Value3 = null;
            resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;
            resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one");

            // Multiple Optional Parameters with supplied values.
            mockResource.Value3 = 600;
            mockResource.Value2 = "value-2";
            resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;
            resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one/600/value-2");

            // Multiple optional Parameters with no supplied value.
            mockResource.Value3 = null;
            mockResource.Value2 = null;
            resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;
            resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one");

            // No route parameters with single parameter populated from posted data.
            resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;
            resource.AssertLink("scenario-5", HttpMethod.Post, "/api/linked/resource/scenario-5/create");

            // Single route parameter with additional class based parameter populated from posted data.
            resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;
            resource.AssertLink("scenario-6", HttpMethod.Put, "/api/linked/resource/scenario-6/10/update");
        }

        /// <summary>
        /// This unit test validates that a resource mapping can specify a URL as a hard-coded string.  This 
        /// approach should be used the least often when specifying resource links that call controller action
        /// methods defined within the boundaries of the same application.  However, this approach can be used
        /// when adding resource links that invoke services provided by other applications.
        /// </summary>
        /// <example>
        /// 
        /// .Map<LinkedResource>()
        ///     .LinkMeta<LinkedResourceController>(
        ///         meta => meta.Href("scenario-20", HttpMethod.Options, "http://external/api/call/info"));
        ///         
        /// </example>
        [Fact]
        public async Task CanGenerateUrl_FromHardCodedString()
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

            var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
            var resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;

            // Assert:
            resource.AssertLink("scenario-20", HttpMethod.Options, "http://external/api/call/info");
        }

        /// <summary>
        /// This unit test validates a resource link mapping that is specified as a hard-coded string but also
        /// contains values corresponding to properties of the resource.  Like the hard-coded scenario, this 
        /// approach should be used when calling external API services.  
        /// 
        /// Unlike the hard-coded string approach, this approach allows you to specify resource properties within 
        /// the generated URL using string interpolation providing compile-time type checking.
        /// </summary>
        /// <example>
        /// 
        /// .Map<LinkedResource>()
        ///      .LinkMeta<LinkedResourceController>(
        ///         meta => meta.Href("scenario-25", HttpMethod.Options, r => $"http://external/api/call/{r.Id}/info/{r.Value2}"));
        ///         
        /// </example>
        [Fact]
        public async Task CanGenerateUrl_FromStringInterpolatedResourceUrl()
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

            var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
            var resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;

            // Assert:
            resource.AssertLink("scenario-25", HttpMethod.Options, "http://external/api/call/10/info/value-2");
        }

        /// <summary>
        /// When specifying resource links within a resource mapping, additional optional metadata can be specified:
        ///     - Name
        ///     - Title
        ///     - Type
        ///     - HrefLang
        /// </summary>
        [Fact]
        public async Task ResourceMap_CanSpecify_AdditionalOptionalLinkProperties()
        {

            // Arrange:
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<LinkedResourceMap>();

            var mockResource = new LinkedResource
            {
                Id = 10,
                Value2 = "value-2"
            };

            var serviceMock = new MockUnitTestService
            {
                ServerResources = new[] { mockResource }
            };

            // Act:
            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, serviceMock);

            var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
            var resource = (await client.SendAsync<LinkedResourceModel>(request)).Content;

            // Assert:
            resource.AssertLink("scenario-30", HttpMethod.Options, "http://external/api/call/10/info/value-2");

            var link = resource.Links["scenario-30"];
            Assert.Equal("test-name", link.Name);
            Assert.Equal("test-title", link.Title);
            Assert.Equal("test-type", link.Type);
            Assert.Equal("test-href-lang", link.HrefLang);
        } 
    }
}
