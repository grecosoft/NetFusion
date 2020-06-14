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
    /// Validates the population of query parameter metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class QueryParameterTests
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
                    RequiredQueryParams(service);
                    OptionalQueryParams(service);
                    OptionalQueryParamsWithDefaults(service);
                });
            });
        }
        
        private static void RequiredQueryParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "RequiredQueryParams", typeof(int), typeof(string));

            actionMetadata.QueryParameters.AssertParamMeta<int>("query-value1");
            actionMetadata.QueryParameters.AssertParamMeta<string>("query-value2");
        }
        
        private static void OptionalQueryParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "OptionalQueryParams", typeof(int), typeof(string));

            actionMetadata.QueryParameters.AssertParamMeta<int?>("query-value1", true);
            actionMetadata.QueryParameters.AssertParamMeta<string>("query-value2", true);
        }
        
        private static void OptionalQueryParamsWithDefaults(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "OptionalQueryParamsWithDefaults", typeof(int), typeof(string));

            actionMetadata.QueryParameters.AssertParamMeta<int?>("query-value1", true, 100);
            actionMetadata.QueryParameters.AssertParamMeta<string>("query-value2", true, "abc");
        }
    }
}