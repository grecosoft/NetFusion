using System.Net.Http;
using System.Threading.Tasks;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Web.Mvc.Metadata;
using WebTests.Hosting;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.ApiMetadata
{
    /// <summary>
    /// Validates the population of route parameter metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class RouteParameterTests
    {
        [Fact]
        public Task ApiMetadata_RouterParams_Populated()
        {
            var mockResource = new MetaResource { Id = 10 };
            
            return WebHostFixture.TestAsync<MetadataController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/documented/actions", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendAsync(request);
                    });

                webResponse.Assert.Service((IApiMetadataService service) =>
                {
                    RequiredRouteParams(service);
                    OptionalRouteParams(service);
                    OptionalRouteParamsWithDefaults(service);
                });
            });
        }
        
        private static void RequiredRouteParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "RequiredRouteParams", typeof(int), typeof(string));
            
            actionMetadata.RouteParameters.AssertParamMeta<int>("id");
            actionMetadata.RouteParameters.AssertParamMeta<string>("v1");
        }

        private static void OptionalRouteParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "OptionalRouteParams", typeof(int), typeof(int?), typeof(string));
                
            actionMetadata.RouteParameters.AssertParamMeta<int>("id");
            actionMetadata.RouteParameters.AssertParamMeta<int?>("v1", true);
            actionMetadata.RouteParameters.AssertParamMeta<string>("v2", true);
        }
            
        private static void OptionalRouteParamsWithDefaults(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "OptionalRouteParamsWithDefaults", typeof(int), typeof(int?), typeof(string));
                
            actionMetadata.RouteParameters.AssertParamMeta<int>("id");
            actionMetadata.RouteParameters.AssertParamMeta<int?>("v1", true, 100);
            actionMetadata.RouteParameters.AssertParamMeta<string>("v2", true, "abc");
        }
    }
}