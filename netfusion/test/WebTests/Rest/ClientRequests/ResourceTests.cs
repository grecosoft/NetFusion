using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Client.Settings;
using NetFusion.Test.Plugins;
using WebTests.Rest.ClientRequests.Client;
using WebTests.Rest.ClientRequests.Server;
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
        public async Task ResourceLinks_NotSerializedWithRequest()
        {
            var mockedSrv = new MockUnitTestService();
            var hostPlugin = new MockAppHostPlugin();

            hostPlugin.AddPluginType<CustomerResourceMap>();

            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, mockedSrv);

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

            var request = ApiRequest.Post("api/customers/pass-through").WithContent(resource);

            await client.SendAsync<CustomerModel>(request);
            var serverReceivedResource = mockedSrv.ServerReceivedResource as CustomerResource;

            serverReceivedResource.Should().NotBeNull("Client Resource Deserialized in Server Resource Representation.");
            serverReceivedResource.CustomerId.Should().Be(resource.CustomerId, "Server Side Serialized Resource Matches Client Resource.");
            serverReceivedResource.Links.Should().BeNull("Links Should only be returned from Server.");
        }

        /// <summary>
        /// When creating new and updating existing resources, the associated embedded resources should
        /// not be serialized back to the server when contained in the request body.
        /// </summary>
        [Fact]
        public async Task EmbeddedResources_NotSerializedWithRequest()
        {
            var settings = RequestSettings.Create(config => config.UseHalDefaults());

            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<CustomerResourceMap>();

            var mockedSrv = new MockUnitTestService();
            var client = settings.CreateTestClient(hostPlugin, mockedSrv);

            // Create a test resource as it would have been returned from the server.
            var resource = new CustomerModel
            {
                CustomerId = Guid.NewGuid().ToString(),
                FirstName = "Doug",
                LastName = "Bowan",
                Age = 77,
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

            var request = ApiRequest.Post("api/customers/pass-through", config => config.WithContent(resource));

            await client.SendAsync<CustomerModel>(request);
            var serverReceivedResource = mockedSrv.ServerReceivedResource as CustomerResource;

            serverReceivedResource.Should().NotBeNull("Client Resource Deserialized in Server Resource Representation.");
            serverReceivedResource.CustomerId.Should().Be(resource.CustomerId, "Server Side Serialized Resource Matches Client Resource.");
            serverReceivedResource.Embedded.Should().BeNull("Embedded Resources Should only be returned from Server.");
        }

        /// <summary>
        /// When receiving a resource from the server using IRequestClient, only the root resource is deserialized.
        /// The root resource will often have one or more embedded resources.  Embedded resources are deserialized 
        /// upon their first request from the underlying generalized serialized format.  All future requests for the
        /// embedded resource return the deserialized instance.
        /// </summary>
        [Fact]
        public async Task ClientCan_ReceiveEmbeddedResource()
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

            var mockSrv = new MockUnitTestService {

                Customers = new[] { serverResource }
            };

            // Create the test client and call route returning an embedded resource.
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<CustomerResourceMap>();

            var client = RequestSettings.Create()
                .CreateTestClient(hostPlugin, mockSrv);

            var request = ApiRequest.Get("api/customers/embedded/resource");

            var response = await client.SendAsync<CustomerModel>(request);

            // Validate that an embedded resource was returned.
            response.Content.Should().NotBeNull();
            response.Content.Embedded.Should().NotBeNull();
            response.Content.Embedded.Keys.Should().HaveCount(1);
            response.Content.Embedded.ContainsKey("primary-address").Should().BeTrue();

            // At this point, the embedded resource is the generic JSON.NET representation.
            // The next line of code will deserialize this generic representation in the C# client side class
            //      matching the server-sided resource.

            var embeddedClientResource = response.Content.GetEmbedded<AddressModel>("primary-address");
            embeddedClientResource.Should().NotBeNull();
            embeddedClientResource.AddressId.Should().Be(embeddedServerResource.AddressId);
        }

        /// <summary>
        /// When receiving a resource from the server using IRequestClient, only the root resource is deserialized.
        /// The root resource will often have one or more embedded  resource collections.  Embedded resource collections
        /// are deserialized upon their first request from the underlying generalized serialized format.  All future 
        /// requests for the embedded resource collection return the deserialized instance.
        /// </summary>
        [Fact]
        public async Task ClientCan_ReceiveEmbeddedResourceCollection()
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

            var embeddedServerResource2 = new AddressResource
            {
                AddressId = Guid.NewGuid().ToString(),
                CustomerId = serverResource.CustomerId
            };

            serverResource.Embed(new[] { embeddedServerResource, embeddedServerResource2 }, "addresses");

            var mockSrv = new MockUnitTestService
            {
                Customers = new[] { serverResource }
            };

            // Create the test client and call route returning an embedded resource collection.
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<CustomerResourceMap>();

            var client = RequestSettings.Create()
                .CreateTestClient(hostPlugin, mockSrv);

            var request = ApiRequest.Create("api/customers/embedded/resource", HttpMethod.Get);
            var response = await client.SendAsync<CustomerModel>(request);

            // Validate that an embedded resource collection was returned.
            response.Content.Should().NotBeNull();
            response.Content.Embedded.Should().NotBeNull();
            response.Content.Embedded.Keys.Should().HaveCount(1);
            response.Content.Embedded.ContainsKey("addresses").Should().BeTrue();

            // At this point, the embedded resource is the generic JSON.NET representation.
            // The next line of code will deserialize this generic representation in the C# client side class
            //      matching the server-sided resource collection.

            var embeddedClientResource = response.Content.GetEmbeddedCollection<AddressModel>("addresses").ToArray();
            embeddedClientResource.Should().NotBeNull();
            embeddedClientResource.Should().HaveCount(2);
        }

        [Fact]
        public async Task ClientSpecified_EnbeddedTypes_SentAsQueryString()
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

            var embeddedServerResource2 = new AddressResource
            {
                AddressId = Guid.NewGuid().ToString(),
                CustomerId = serverResource.CustomerId
            };

            var embeddedServerResource3 = new AddressResource
            {
                AddressId = Guid.NewGuid().ToString(),
                CustomerId = serverResource.CustomerId
            };

            serverResource.Embed(new[] { embeddedServerResource, embeddedServerResource2 }, "addresses");
            serverResource.Embed(embeddedServerResource3, "vacation-address");

            var mockSrv = new MockUnitTestService
            {
                Customers = new[] { serverResource }
            };

            // Create the test client and call route returning an embedded resource collection.
            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<CustomerResourceMap>();

            var client = RequestSettings.Create()
                .CreateTestClient(hostPlugin, mockSrv);

            var request = ApiRequest.Create("api/customers/embedded/resource", HttpMethod.Get).Embed("vacation-address");
            var response = await client.SendAsync<CustomerModel>(request);

            response.Request.RequestUri.Query.Should().Equals("?embed=vacation-address");

            // If supported by the service then only one embedded resource should have been returned.
            response.Content.Embedded.Should().HaveCount(1);
            response.Content.Embedded.ContainsKey("vacation-address").Should().BeTrue();
        }
    }
}
