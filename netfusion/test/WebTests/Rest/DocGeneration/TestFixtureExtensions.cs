using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Docs.Plugin.Configs;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Test.Plugins;
using WebTests.Hosting;

namespace WebTests.Rest.DocGeneration
{
    public static class TestFixtureExtensions
    {
        public static WebServerConfig ArrangeForRestDocs(this WebHostFixture fixture)
        {
            return fixture.WithServices(services =>
            {

            })
                .ComposedFrom(compose =>
                {
                    compose.AddRest();
                    compose.AddRestDocs();

                    var hostPlugin = new MockHostPlugin();
                    // hostPlugin.AddPluginType<LinkedResourceMap>();

                    compose.AddPlugin(hostPlugin);
                });
        }

        public static string GetDocUrl(this string actionUrl) =>
            $"{new RestDocConfig().EndpointUrl}?doc={actionUrl}";

        public static async Task<ApiActionDoc> AsApiActionDocAsync(this HttpResponseMessage response)
        {
            return JsonSerializer.Deserialize<ApiActionDoc>(await response.Content.ReadAsStringAsync());
        }
    }
}
