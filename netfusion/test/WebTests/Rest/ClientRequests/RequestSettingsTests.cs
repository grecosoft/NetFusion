using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using WebTests.Hosting;
using WebTests.Rest.LinkGeneration;
using WebTests.Rest.Setup;
using Xunit;
using CustomerModel = WebTests.Rest.ClientRequests.Server.CustomerModel;

namespace WebTests.Rest.ClientRequests
{
    /// <summary>
    /// Unit tests for specifying header values.  Header values can be specified at the HttpClient
    /// level and used by all requests made by the client.  The settings can also be specified for
    /// a specific request.  When specified for a specific request, the request settings override
    /// any corresponding settings specified at the client level.
    /// </summary>
    public class RequestSettingsTests
    {
        /// <summary>
        /// A convenience method can be called to configure the settings commonly used
        /// for making requests to HAL based server.
        /// </summary>
        [Fact]
        public void CanSpecifyKnown_HalDefaultSettings()
        {
            var settings = RequestSettings.Create(config => config.UseHalDefaults());
            settings.Headers.Accept.Should().HaveCount(2);

            var request = new HttpRequestMessage();
            settings.Apply(request);

            var acceptHeader = request.Headers.Accept.ToString();
            acceptHeader.Should().Be("application/hal+json, application/json");
        }

        /// <summary>
        /// The header name must be specified.
        /// </summary>
        [Fact]
        public void AddingHeader_MustHaveName()
        {
            RequestSettings.Create(config =>
            {
                Assert.Throws<ArgumentException>(
                    () => config.Headers.Add(null, InternetMediaTypes.Json));

                Assert.Throws<ArgumentException>(
                    () => config.Headers.Add("", InternetMediaTypes.Json));

                Assert.Throws<ArgumentException>(
                    () => config.Headers.Add("  ", InternetMediaTypes.Json));

            });
        }

        /// <summary>
        /// A header can contain an array of associated string values.  At least one value
        /// must be specified.
        /// </summary>
        [Fact]
        public void AddingHeader_MustHaveAtLeastOneValue()
        {
            RequestSettings.Create(config =>
            {
                Assert.Throws<ArgumentException>(
                    () => config.Headers.Add("a", null));

                Assert.Throws<ArgumentException>(
                    () => config.Headers.Add("a", new string[] { }));

            });
        }

        /// <summary>
        /// Certain known header types such as Accept can have a quality associated with the value
        /// representing the precedence of the value.
        /// </summary>
        [Fact]
        public void AddingHeaderWithQuality_MustBeGreaterThanZero()
        {
            RequestSettings.Create(config =>
            {
                Assert.Throws<ArgumentException>(
                    () => config.Headers.AcceptMediaType("a", -1));
            });
        }

        /// <summary>
        /// Multiple header values can be specified.
        /// </summary>
        [Fact]
        public void Header_CanHaveMultipleValues()
        {
            var settings = RequestSettings.Create(config =>
            {
                config.Headers
                    .Add("a", "v1", "v2")
                    .Add("b", "v3");
            });

            var request = new HttpRequestMessage();
            settings.Apply(request);

            request.Headers.ToString().Should().Be("a: v1, v2\r\nb: v3\r\n");
        }

        /// <summary>
        /// Multiple Accept header values can be specified with specific assigned quality.
        /// </summary>
        [Fact]
        public void AcceptHeader_CanHaveMultipleValuesWithQuality()
        {
            var settings = RequestSettings.Create(config =>
            {
                config.Headers.AcceptMediaType(InternetMediaTypes.HalJson, 0.5);
                config.Headers.AcceptMediaType(InternetMediaTypes.Json, 0.9);
            });

            settings.Headers.Accept.Should().HaveCount(2);

            var request = new HttpRequestMessage();
            settings.Apply(request);

            var acceptHeader = request.Headers.Accept.ToString();
            acceptHeader.Should().Be("application/hal+json; q=0.5, application/json; q=0.9");
        }

        /// <summary>
        /// Query string parameters to be added to the request URL can be specified.
        /// </summary>
        [Fact]
        public Task QueryStringParams_CanBeSpecified()
        {
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                IMockedService mockedSrv = new MockUnitTestService
                {
                    Customers = new[] { new CustomerModel { CustomerId = "ID_1", Age = 47 } }
                };

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
                        
                        return await client.SendAsync<Client.CustomerModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {  
                    apiResponse.Request.RequestUri.PathAndQuery
                        .Should().Be("/api/customers/24234234234?a=v1&b=v2");
                });
            });
        }
    }
}
