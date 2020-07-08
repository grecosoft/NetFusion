using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Docs.Plugin.Configs;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Test.Plugins;
using WebTests.Hosting;
using WebTests.Rest.DocGeneration.Server;

namespace WebTests.Rest.DocGeneration
{
    public static class TestFixtureExtensions
    {
        public static WebServerConfig ArrangeForRestDocs(this WebHostFixture fixture)
        {
            return fixture.WithServices(services =>
            {

            })
            .UsingAppSerivces(appServices => appServices.UseRestDocs())
            .ComposedFrom(compose =>
            {
                compose.AddRest();
                compose.AddRestDocs();

                var hostPlugin = new MockHostPlugin();
                hostPlugin.AddPluginType<DocResourceMap>();

                compose.AddPlugin(hostPlugin);
            });
        }

        public static string GetDocUrl(this string actionUrl) =>
            $"{new RestDocConfig().EndpointUrl}?doc={actionUrl}";

        public static async Task<ApiActionDoc> AsApiActionDocAsync(this HttpResponseMessage response)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Http Request was not Successful: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<ApiActionDoc>(await response.Content.ReadAsStringAsync(), options);
        }
    }
}
