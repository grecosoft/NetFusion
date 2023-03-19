using System;
using System.Net.Http;
using System.Threading.Tasks;
using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Client;
using NetFusion.Web.Rest.Client.Settings;
using NetFusion.Web.UnitTests.Hosting;
using NetFusion.Web.UnitTests.Rest.ApiMetadata.Server;
using NetFusion.Web.UnitTests.Rest.Setup;

namespace NetFusion.Web.UnitTests.Rest.ApiMetadata.Setup;

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