using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Resources.Hal;
using WebTests.Hosting;
using WebTests.Rest.ClientRequests.Client;
using WebTests.Rest.ClientRequests.Server;
using WebTests.Rest.LinkGeneration;
using WebTests.Rest.Setup;
using Xunit;
using AddressModel = WebTests.Rest.ClientRequests.Server.AddressModel;
using CustomerModel = WebTests.Rest.ClientRequests.Server.CustomerModel;

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
            // Arrange the test resources that will be returned from the server
            // to test the client consumer code.
            var serverResource = new CustomerModel
            {
                CustomerId = Guid.NewGuid().ToString()
            };

            var embeddedServerResource = new AddressModel
            {
                AddressId = Guid.NewGuid().ToString(),
                CustomerId = serverResource.CustomerId
            };

          //  serverResource.Embed(embeddedServerResource, "primary-address");
            
            // Configure the backend test-service to return the resource.
            var mockSrv = new MockUnitTestService {

                Customers = new[] { serverResource }
            };
            
            // Run the unit test and request the resource and assert the expected results.
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockSrv)
                    
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Get("api/customers/embedded/resource");
                        return await client.SendAsync<Client.CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (HalResource<Client.CustomerModel>)apiResponse.Content;
                    
                    // Validate that an embedded resource was returned.
                    apiResponse.Content.Should().NotBeNull();
                    resource.Embedded.Should().NotBeNull();
                    resource.Embedded.Keys.Should().HaveCount(1);
                    resource.Embedded.ContainsKey("primary-address").Should().BeTrue();

                    // At this point, the embedded resource is the generic JSON.NET representation.
                    // The next line of code will deserialize this generic representation in the C# client side class
                    //      matching the server-sided resource.

                    var embeddedClientResource = resource.GetEmbedded<Client.AddressModel>("primary-address");
                    embeddedClientResource.Should().NotBeNull();
                    embeddedClientResource.Model.AddressId.Should().Be(embeddedServerResource.AddressId);
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
            // Arrange the test resources that will be returned from the server
            // to test the client consumer code.
            var serverResource = new CustomerModel
            {
                CustomerId = Guid.NewGuid().ToString()
            };

            // Embed to child resources.
//            serverResource.Embed(new[] { 
//                new AddressResource { AddressId = Guid.NewGuid().ToString(), CustomerId = serverResource.CustomerId },
//                new AddressResource { AddressId = Guid.NewGuid().ToString(), CustomerId = serverResource.CustomerId }
//            }, "addresses");

            // Configure the mock service to return the resource.
            var mockSrv = new MockUnitTestService
            {
                Customers = new[] { serverResource }
            };

            // Run the unit test and request the resource and assert the expected results.
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockSrv)
                    
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Get("api/customers/embedded/resource");
                        return await client.SendAsync<Client.CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (Client.CustomerModel)apiResponse.Content;
                    
//                    // Validate that an embedded resource collection was returned.
//                    resource.Should().NotBeNull();
//                    resource.Embedded.Should().NotBeNull();
//                    resource.Embedded.Keys.Should().HaveCount(1);
//                    resource.Embedded.ContainsKey("addresses").Should().BeTrue();
//
//                    // At this point, the embedded resource is the generic JSON.NET representation.
//                    // The next line of code will deserialize this generic representation in the C# client side class
//                    //      matching the server-sided resource collection.
//
//                    var embeddedClientResource = resource.GetEmbeddedCollection<Client.AddressModel>("addresses").ToArray();
//                    embeddedClientResource.Should().NotBeNull();
//                    embeddedClientResource.Should().HaveCount(2);
                    
                });
            });
        }
    }
}
