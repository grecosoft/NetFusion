using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using Xunit;

namespace WebTests.Rest.ClientRequests
{
    /// <summary>
    /// Unit tests for specifying header values.  Header values can be specified at the HttpClient
    /// level and used by all requests made by the client.  The settings can also be specified for
    /// a specific request.  When specified for a specific request, the request settings override
    /// any corresponding settings specified at the client level.
    /// </summary>
    public class RequestHeaderTests
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
                    () => config.Headers.Add("a"));
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

            request.Headers.ToString().Should().Be("a: v1, v2\nb: v3\n");
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
        /// Certain known headers such as Accept and Content-Type must be specified using
        /// method provided by the RequestHeader class.  If the Add method is used, and
        /// exception will be raised.
        /// </summary>
        [Fact]
        public void KnowHeaderValues_MustBeSet_UsingExplicitMethod()
        {
            RequestSettings.Create(config =>
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    config.Headers.Add("Accept", "application/json");
                }).Message.Should().Contain("must be set using the specific");
                
                Assert.Throws<ArgumentException>(() =>
                {
                    config.Headers.Add("Content-Type", "application/json");
                }).Message.Should().Contain("must be set using the specific");
            });
        }

        /// <summary>
        /// An existing header value can be removed.
        /// </summary>
        [Fact]
        public void HeaderValue_CanBeRemoved()
        {
            var settings = RequestSettings.Create(config =>
            {
                config.Headers.Add("a", "test");
            });

            settings.Headers.Values.ContainsKey("a").Should().BeTrue();
            settings.Headers.Remove("a");
            settings.Headers.Values.ContainsKey("a").Should().BeFalse();
        }

        /// <summary>
        /// If an authorization header was previously set, it can be removed.
        /// </summary>
        [Fact]
        public void AuthorizationHeader_CanBeRemoved()
        {
            var settings = RequestSettings.Create(config =>
            {
                config.Headers.SetBasicAuthHeader("tick-tock", "fruit-cake");
            });

            settings.Headers.Values.ContainsKey("Authorization").Should().BeTrue();
            settings.Headers.RemoveAuthHeader();
            settings.Headers.Values.ContainsKey("Authorization").Should().BeFalse();
        }

        /// <summary>
        /// A method exists so that a Bearer authorization token header can be set.
        /// </summary>
        [Fact]
        public void BearerAuthorizationHeader_CanBeSet()
        {
            var settings = RequestSettings.Create(config =>
            {
                config.Headers.SetAuthBearerToken("XXX_YYY_ZZZ");
            });

            settings.Headers.Values.ContainsKey("Authorization").Should().BeTrue();
            settings.Headers.Values.Single(h => h.Key == "Authorization").Value
                .Value.Should().Contain("Bearer XXX_YYY_ZZZ");
        }

        /// <summary>
        /// Common set of headers can be merged into the headers of a request.
        /// If the header exists on the request, it has persistence.
        /// </summary>
        [Fact]
        public void CommonSetOfSettings_CanBeMerged()
        {
            var request = ApiRequest.Create("api/test", HttpMethod.Patch, config =>
            {
                config.Settings.Headers.Add("a", "123");
                config.Settings.Headers.Add("b", "xyz");
            });

            var commonSettings = RequestSettings.Create(config =>
            {
                config.Headers.Add("a", "---");
                config.Headers.Add("c", "987");
            });

            request.Merge(commonSettings);
            request.Settings.Should().NotBeNull();
            request.Settings.Headers.Should().NotBeNull();
            request.Settings.Headers.Values.Should().HaveCount(3);

            request.Settings.Headers.Values["a"].Value.First().Should().Be("123");
            request.Settings.Headers.Values["b"].Value.First().Should().Be("xyz");
            request.Settings.Headers.Values["c"].Value.First().Should().Be("987");
        }

        [Fact]
        public void CanTestResponse_ForAuthChallenge()
        {
            var httpResp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            };
            
            httpResp.Headers.TryAddWithoutValidation("WWW-Authenticate", "realm=http://www.get/token/here");
            
            var apiResponse = new ApiResponse(httpResp);
            apiResponse.IsAuthChallenge().Should().BeTrue();
        }

        [Fact]
        public void CanGetAuthChallenge_RealmUrlValue()
        {
            var httpResp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            };
            
            httpResp.Headers.TryAddWithoutValidation("WWW-Authenticate", "realm=http://www.get/token/here");
            
            var apiResponse = new ApiResponse(httpResp);
            apiResponse.GetAuthRealmUrl().Should().Be("http://www.get/token/here");
        }

        [Fact]
        public void CanGetAuthChallenge_OnlyForStatus401()
        {
            var httpResp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            };

            var apiResponse = new ApiResponse(httpResp);
            Assert.Throws<InvalidOperationException>(
                () => apiResponse.GetAuthRealmUrl()
            ).Message.Should().Contain("WWW-Authenticate header not found.");
        }

        [Fact]
        public void CanGet_XCustomToken_HeaderValue()
        {
            var httpResp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            
            httpResp.Headers.TryAddWithoutValidation("X-Custom-Token", "ASDFASDDGASDFASDF");
            
            var apiResponse = new ApiResponse(httpResp);
            apiResponse.GetAuthToken().Should().Be("ASDFASDDGASDFASDF");


        }
    }
}
