using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using WebTests.Hosting;
using WebTests.Rest.ClientRequests.Server;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.ClientRequests
{
    public class ResourceTests
    {
        /// <summary>
        /// When receiving a resource from the server using IRequestClient, only the root resource is deserialized.
        /// The root resource will often have one or more embedded resources.  Embedded resources are deserialized 
        /// upon their first request from the underlying generalized serialized format.  All future requests for the
        /// embedded resource return the deserialized instance.
        /// </summary>
        [Fact]
        public Task ClientCan_ReceiveEmbeddedResource()
        {
            return WebHostFixture.TestAsync<CustomerController>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults()
                    
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Get("api/customers/embedded/resource");
                        return await client.SendAsync<CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resourceResponse = (ApiResponse<CustomerModel>)apiResponse;
                    var resource = resourceResponse.Resource;
                    
                    // Validate that an embedded resource was returned.
                    apiResponse.Content.Should().NotBeNull();
                    resource.Embedded.Should().NotBeNull();
                    resource.Embedded.Keys.Should().HaveCount(1);
                    resource.Embedded.ContainsKey("primary-address").Should().BeTrue();

                    var embeddedClientResource = resource.GetEmbeddedResource<AddressModel>("primary-address");
                    embeddedClientResource.Should().NotBeNull();
                    embeddedClientResource.Model.AddressId.Should().NotBeNull();
                });
            });
        }

        /// <summary>
        /// When receiving a resource from the server using IRequestClient, only the root resource is deserialized.
        /// The root resource will often have one or more embedded  resource collections.  Embedded resource collections
        /// are deserialized upon their first request from the underlying generalized serialized format.  All future 
        /// requests for the embedded resource collection return the deserialized instance.
        /// </summary>
        [Fact]
        public Task ClientCan_ReceiveEmbeddedResourceCollection()
        {
            return WebHostFixture.TestAsync<CustomerController>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults()
                    
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Get("api/customers/embedded/collection");
                        return await client.SendAsync<CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resourceResponse = (ApiResponse<CustomerModel>)apiResponse;
                    var resource = resourceResponse.Resource;
                    
                    // Validate that an embedded resource collection was returned.
                    resource.Should().NotBeNull();
                    resource.Embedded.Should().NotBeNull();
                    resource.Embedded.Keys.Should().HaveCount(1);
                    resource.Embedded.ContainsKey("addresses").Should().BeTrue();

                    var embeddedClientResource = resource.GetEmbeddedResources<AddressModel>("addresses").ToArray();
                    embeddedClientResource.Should().NotBeNull();
                    embeddedClientResource.Should().HaveCount(2);
                    
                });
            });
        }
    }
}
