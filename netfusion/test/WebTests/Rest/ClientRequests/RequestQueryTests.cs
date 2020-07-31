using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using WebTests.Hosting;
using WebTests.Mocks;
using WebTests.Rest.Setup;
using Xunit;

namespace WebTests.Rest.ClientRequests
{
    public class RequestQueryTests
    {
        /// <summary>
        /// Query string parameters to be added to the request URL can be specified.
        /// </summary>
        [Fact]
        public Task QueryStringParams_CanBeSpecified()
        {
            return WebHostFixture.TestAsync<LinkClientTests>(async host =>
            {
                IMockedService mockedSrv = new MockUnitTestService();

                var response = await host
                    .ArrangeWithDefaults(mockedSrv)
                    
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/customers/24234234234", HttpMethod.Get,
                            config =>
                            {
                                config.Settings.UseHalDefaults();
                                config.Settings.QueryString
                                    .AddParam("a", "v1")
                                    .AddParam("b", "v2");
                            });
                        
                        return await client.SendAsync(request);
                    });

                // Assert the path of the underlying request that was sent to the server.
                response.Assert.ApiResponse(apiResponse =>
                {  
                    apiResponse.Request.RequestUri.PathAndQuery
                        .Should().Be("/api/customers/24234234234?a=v1&b=v2");
                });
            });
        }

        /// <summary>
        /// A common set of query string value can be merged into a request.
        /// If a query string named parameter is defined at the request level,
        /// it has precedence.
        /// </summary>
        [Fact]
        public void QueryParameters_CanBeMerged_InfoRequest()
        {
            var request = ApiRequest.Get("api/test", config =>
            {
                config.Settings.QueryString.AddParam("a", "1");
                config.Settings.QueryString.AddParam("b", "2");
            });

            var settings = RequestSettings.Create(config =>
            {
                config.QueryString.AddParam("b", "---");
                config.QueryString.AddParam("c", "3");
            });

            request.Merge(settings);
            request.Settings.QueryString.Params.Should().HaveCount(3);
            request.Settings.QueryString.Params["a"].Should().Be("1");
            request.Settings.QueryString.Params["b"].Should().Be("2");
            request.Settings.QueryString.Params["c"].Should().Be("3");
        }
    }
}