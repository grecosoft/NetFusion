using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Test.Plugins;
using WebTests.Rest.LinkGeneration.Client;
using WebTests.Rest.LinkGeneration.Server;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.ClientRequests
{
    public class ResultClientTests
    {  
        [Fact]
        public async Task IfServerException_ExceptionRaised()
        {
            // Arrange:
            var hostPlugin = new MockHostPlugin();
          
            var serviceMock = new MockUnitTestService
            {
                TriggerServerSideException = true
            };

            // Act:
            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, serviceMock);

            var request = ApiRequest.Create("api/error/server/side", HttpMethod.Get);

            var exception = await Record.ExceptionAsync (() => client.SendAsync(request));
            exception.Should().NotBeNull();

            if (exception == null) return;
            
            exception.Should().BeOfType<InvalidOperationException>();
            exception.Message.Should().Be("Test-Exception");
        }

        [Fact]
        public async Task IfErrorStatusCode_SuccessStatus_SetCorrectly()
        {
            // Arrange:
            var hostPlugin = new MockHostPlugin();

            var serviceMock = new MockUnitTestService
            {
                 ReturnsStatusCode = StatusCodes.Status404NotFound
            };

            // Act:
            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, serviceMock);

            var request = ApiRequest.Create("api/error/server/http/error-code", HttpMethod.Get);

            var response = await client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.ReasonPhase.Should().Be("Not Found");
          
        }

        [Fact]
        public async Task Response_DescriptiveProperties_SetCorrectly()
        {
            // Arrange:
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<LinkedResourceMap>();

            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };

            var serviceMock = new MockUnitTestService
            {
                ServerResources = new[] { mockResource }
            };

            // Act:
            var client = RequestSettings.Create()
               .CreateTestClient(hostPlugin, serviceMock);

            var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);

            var resource = await client.SendAsync<LinkedResourceModel>(request);
            resource.MediaType.Should().Be("application/hal+json");
            resource.CharSet.Should().Be("utf-8");
            resource.ContentLength.Should().Be(1108);
        }

      
    }
}
