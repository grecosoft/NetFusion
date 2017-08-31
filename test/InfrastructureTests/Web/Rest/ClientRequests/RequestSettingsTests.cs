using FluentAssertions;
using InfrastructureTests.Web.Rest.ClientRequests.Client;
using InfrastructureTests.Web.Rest.ClientRequests.Server;
using InfrastructureTests.Web.Rest.LinkGeneration.Client;
using InfrastructureTests.Web.Rest.LinkGeneration.Server;
using InfrastructureTests.Web.Rest.Setup;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using NetFusion.Test.Plugins;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace InfrastructureTests.Web.Rest.ClientRequests
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
            var settings = RequestSettings.Create(config =>
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
            var settings = RequestSettings.Create(config =>
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
            var settings = RequestSettings.Create(config =>
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

            request.Headers.ToString().Should().Equals("a: v1, v2\r\nb: v3\r\n");
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
            acceptHeader.Should().Equals("application/hal+json; q=0.5, application/json; q=0.9");
        }

        /// <summary>
        /// When registering a base address, the default settings to be applied for each made request to that
        /// base address can be specified.  If specific settings are specified for a given request, they are
        /// merged into the default settings.
        /// </summary>
        [Fact]
        public async Task RequestSpecificSettings_MergedWithBaseAddressDefaultSettings()
        {
            var defaultSettings = RequestSettings.Create(config => {
                config.Headers
                    .Add("h1", "dv1", "dv2")
                    .Add("h2", "dv3");

                config.QueryString
                    .AddParam("p1", "pv1")
                    .AddParam("p2", "pv2");
            });

            var reqestSpecificSettings = RequestSettings.Create(config => {
                config.Headers
                    .Add("h1", "rs100")
                    .Add("h3", "rs200");

                config.QueryString
                    .AddParam("p2", "pv300")
                    .AddParam("p3", "pv400");
            });

            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<CustomerResourceMap>();

            var client = defaultSettings.CreateTestClient(hostPlugin);

            var request = ApiRequest.Get("api/customers/pass-through").UsingSettings(reqestSpecificSettings);
            var response = await client.Send<CustomerModel>(request);

            response.Request.RequestUri.PathAndQuery
                .Should().Be("/api/customers/pass-through?p1=pv1&p2=pv300&p3=pv400");

            response.Request.Headers.ToString()
                .Should().Equals("Accept: application/hal+json, application/json\\r\\nh1: rs100\\r\\nh2: dv3\\r\\nh3: rs200\\r\\nHost: localhost\\r\\n");
        }

        /// <summary>
        /// Query string parameters to be added to the request URL can be specified.
        /// </summary>
        [Fact]
        public async Task QueryStringParams_CanBeSpecified()
        {
            var settings = RequestSettings.Create(config =>
            {
                config.UseHalDefaults();

                config.QueryString
                    .AddParam("a", "v1")
                    .AddParam("b", "v2");
            });

            IMockedService mockedSrv = new MockUnitTestService
            {
                Customers = new[] { new CustomerResource { CustomerId = "ID_1", Age = 47 } }
            };

            var hostPlugin = new MockAppHostPlugin();
            hostPlugin.AddPluginType<CustomerResourceMap>();

            var client = settings.CreateTestClient(hostPlugin, mockedSrv);

            var request = ApiRequest.Create("api/customers/24234234234", HttpMethod.Get);
            var response = await client.Send<CustomerModel>(request);

            response.Request.RequestUri.PathAndQuery
                .Should().Be("/api/customers/24234234234?a=v1&b=v2");
        }

       
    }
}
