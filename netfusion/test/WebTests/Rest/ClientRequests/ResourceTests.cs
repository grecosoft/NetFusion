using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Resources;
using NetFusion.Test.Hosting;
using WebTests.Rest.ClientRequests.Client;
using WebTests.Rest.ClientRequests.Server;
using WebTests.Rest.LinkGeneration;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.ClientRequests
{
    public class ResourceTests
    {
        /// <summary>
        /// When creating new and updating existing resources, the associated link should not be
        /// serialized back to the server when contained in the request body.
        /// </summary>
        [Fact]
        public Task ResourceLinks_NotSerializedWithRequest()
        {
            // Create a test resource as it would have been returned from the server.
            var resource = new CustomerModel {
                CustomerId = Guid.NewGuid().ToString(),
                FirstName = "Doug",
                LastName = "Bowan",
                Age = 77,
                Links = new Dictionary<string, Link> {
                    { "test:lj4000", new Link { Href = "pull/rip/cord" } }
                }
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults()
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Post("api/customers/pass-through").WithContent(resource);

                        return await client.SendAsync<CustomerModel>(request);
                    });

                response.Assert.Service<IMockedService>(mockedSrv =>
                {
                    var serverReceivedResource = mockedSrv.ServerReceivedResource as CustomerResource;
                     
                    serverReceivedResource.Should().NotBeNull("Client Resource Deserialized in Server Resource Representation.");
                    if (serverReceivedResource == null) return;
            
                    serverReceivedResource.CustomerId.Should().Be(resource.CustomerId, "Server Side Serialized Resource Matches Client Resource.");
                    serverReceivedResource.Links.Should().BeNull("Links Should only be returned from Server.");
                });
            });
        }

        /// <summary>
        /// When creating new and updating existing resources, the associated embedded resources should
        /// not be serialized back to the server when contained in the request body.
        /// </summary>
        [Fact]
        public Task EmbeddedResources_NotSerializedWithRequest()
        {
            // Create a test resource as it would have been returned from the server.
            var resource = new CustomerModel {
                CustomerId = Guid.NewGuid().ToString(),
                FirstName = "Doug",
                LastName = "Bowan",
                Age = 77
            };
            
            // Create an embedded resource.  This would be the same state as if 
            // the embedded resource, returned from the server, was deserialized into
            // the client-side resource representation.
            var embeddedResource = new AddressModel {
                AddressId = Guid.NewGuid().ToString(),
                CustomerId = resource.CustomerId,
                City = "Buckhannon",
                Street = "59 College Avenue",
                ZipCode = "26201"
            };

            resource.Embedded = new Dictionary<string, object>();
            resource.Embedded.Add("address", embeddedResource);
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults()
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Post("api/customers/pass-through", config => config.WithContent(resource));
                        return await client.SendAsync<CustomerModel>(request);
                    });

                response.Assert.Service<IMockedService>(mockedSrv =>
                {
                    var serverReceivedResource = mockedSrv.ServerReceivedResource as CustomerResource;

                    serverReceivedResource.Should().NotBeNull("Client Resource Deserialized in Server Resource Representation.");
                    serverReceivedResource.CustomerId.Should().Be(resource.CustomerId, "Server Side Serialized Resource Matches Client Resource.");
                    serverReceivedResource.Embedded.Should().BeNull("Embedded Resources Should only be returned from Server.");
                });
            });
        }

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
            var serverResource = new CustomerResource
            {
                CustomerId = Guid.NewGuid().ToString()
            };

            var embeddedServerResource = new AddressResource
            {
                AddressId = Guid.NewGuid().ToString(),
                CustomerId = serverResource.CustomerId
            };

            serverResource.Embed(embeddedServerResource, "primary-address");
            
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
                        return await client.SendAsync<CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (CustomerModel)apiResponse.Content;
                    
                    // Validate that an embedded resource was returned.
                    apiResponse.Content.Should().NotBeNull();
                    resource.Embedded.Should().NotBeNull();
                    resource.Embedded.Keys.Should().HaveCount(1);
                    resource.Embedded.ContainsKey("primary-address").Should().BeTrue();

                    // At this point, the embedded resource is the generic JSON.NET representation.
                    // The next line of code will deserialize this generic representation in the C# client side class
                    //      matching the server-sided resource.

                    var embeddedClientResource = resource.GetEmbedded<AddressModel>("primary-address");
                    embeddedClientResource.Should().NotBeNull();
                    embeddedClientResource.AddressId.Should().Be(embeddedServerResource.AddressId);
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
            var serverResource = new CustomerResource
            {
                CustomerId = Guid.NewGuid().ToString()
            };

            // Embed to child resources.
            serverResource.Embed(new[] { 
                new AddressResource { AddressId = Guid.NewGuid().ToString(), CustomerId = serverResource.CustomerId },
                new AddressResource { AddressId = Guid.NewGuid().ToString(), CustomerId = serverResource.CustomerId }
            }, "addresses");

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
                        return await client.SendAsync<CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (CustomerModel)apiResponse.Content;
                    
                    // Validate that an embedded resource collection was returned.
                    resource.Should().NotBeNull();
                    resource.Embedded.Should().NotBeNull();
                    resource.Embedded.Keys.Should().HaveCount(1);
                    resource.Embedded.ContainsKey("addresses").Should().BeTrue();

                    // At this point, the embedded resource is the generic JSON.NET representation.
                    // The next line of code will deserialize this generic representation in the C# client side class
                    //      matching the server-sided resource collection.

                    var embeddedClientResource = resource.GetEmbeddedCollection<AddressModel>("addresses").ToArray();
                    embeddedClientResource.Should().NotBeNull();
                    embeddedClientResource.Should().HaveCount(2);
                    
                });
            });
        }
    }
}
