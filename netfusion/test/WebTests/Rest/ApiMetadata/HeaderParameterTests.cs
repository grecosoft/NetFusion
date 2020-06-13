using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
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
                });
            });
        }
        
        private static void RequiredHeaderParams(IApiMetadataService apiMetadata)
        {
            var actionMetadata = apiMetadata.GetActionMeta<MetadataController>(
                "RequiredHeaderParams", typeof(int));

      
        }

        
            
       
        private static void AssertParam<T>(ApiActionMeta actionMeta, string name, 
            bool isOptional = false, 
            object defaultValue = null)
        {
            actionMeta.Should().NotBeNull();
                
            var paramMeta = actionMeta.RouteParameters.Single(p => p.ParameterName == name);
            paramMeta.Should().NotBeNull();
            paramMeta.IsOptional.Should().Be(isOptional);
            paramMeta.ParameterType.Should().Be(typeof(T));

            if (defaultValue != null)
            {
                paramMeta.DefaultValue.Should().Be(defaultValue);
            }
        } 
    }
}