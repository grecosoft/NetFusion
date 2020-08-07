using System;
using System.Net.Http;
using System.Threading.Tasks;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Web.Mvc.Metadata;
using WebTests.Hosting;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.Setup;

namespace WebTests.Rest.ApiMetadata.Setup
{
    public static class TestWebHostSetup
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
    }
}