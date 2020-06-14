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
    /// Validates the population of header parameter metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class HeaderParameterTests
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
                    RequiredHeaderParams(service);
                    OptionalHeaderParams(service);
                    OptionalHeaderParamsWithDefaults(service);
                });
            });
        }
        
        private static void RequiredHeaderParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "RequiredHeaderParams", typeof(int), typeof(string));

            actionMetadata.HeaderParameters.AssertParamMeta<int>("header-value1");
            actionMetadata.HeaderParameters.AssertParamMeta<string>("header-value2");
        }
        
        private static void OptionalHeaderParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "OptionalHeaderParams", typeof(int), typeof(string));

            actionMetadata.HeaderParameters.AssertParamMeta<int?>("header-value1", true);
            actionMetadata.HeaderParameters.AssertParamMeta<string>("header-value2", true);
        }
        
        private static void OptionalHeaderParamsWithDefaults(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "OptionalHeaderParamsWithDefaults", typeof(int), typeof(string));

            actionMetadata.HeaderParameters.AssertParamMeta<int?>("header-value1", true, 100);
            actionMetadata.HeaderParameters.AssertParamMeta<string>("header-value2", true, "abc");
        }
    }
}