using System;
using System.Collections.Generic;
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

namespace WebTests.Rest.ApiMetadata
{
    public static class TestApiMetadata
    {
        // Helper method that will run a provided action within a configured web host.
        // The test method is passed and instance of IApiMetadataService that can be
        // used to assert the results.
        public static Task Run(Action<IApiMetadataService> test)
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
                    test.Invoke(service);
                });
            });
        }
        
        public static void AssertParamMeta<T>(this IEnumerable<ApiParameterMeta> paramsMeta, string name, 
            bool isOptional = false, 
            object defaultValue = null)
        {
            var paramMeta = paramsMeta.Single(p => p.ParameterName == name);
            paramMeta.Should().NotBeNull();
            paramMeta.IsOptional.Should().Be(isOptional);
            paramMeta.ParameterType.Should().Be(typeof(T));

            if (defaultValue != null)
            {
                paramMeta.DefaultValue.Should().Be(defaultValue);
            }
        }

        public static void AssertResponseMeta(this IEnumerable<ApiResponseMeta> responseMeta, int statusCode,
            Type responseType = null)
        {
            responseMeta.Where(m => m.Status == statusCode && m.ModelType == responseType)
                .Should().HaveCount(1);
        }
    }
}