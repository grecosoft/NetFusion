using System.Linq;
using System.Net.Http;
using NetFusion.Rest.Client;
using NetFusion.Rest.Resources.Hal;
using Xunit;

namespace WebTests.Rest
{
    public static class ClientResourceTestExtensions
    {
        public static void AssertLink(this IHalResource resource,
            string relName,
            HttpMethod expectedMethod,
            string expectedValue)
        {
            Assert.True(
                resource.Links != null && resource.Links.Count > 0,
                "Resource does not have associated links.");

            Assert.True(resource.Links.ContainsKey(relName), $"Resource does not have link with relation name: {relName}");

            var link = resource.Links[relName];
            Assert.Equal(expectedValue, link.Href);
            Assert.NotNull(link.Methods);
            Assert.True(link.Methods.Length == 1, "One HTTP method expected.");
            Assert.Equal(expectedMethod.Method, link.Methods.First());
        }

        public static void AssertRequest(this ApiRequest request,
            string expectedRequestUri,
            HttpMethod expectedMethod)
        {
            Assert.Equal(expectedRequestUri, request.RequestUri);
            Assert.Equal(expectedMethod, request.Method);
        }
    }
}
