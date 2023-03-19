using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Web.UnitTests.Hosting;
using NetFusion.Web.UnitTests.Rest.CodeGeneration.Setup;
using NetFusion.Web.UnitTests.Rest.DocGeneration.Server;
using Xunit;

namespace NetFusion.Web.UnitTests.Rest.CodeGeneration;

public class CodeGenerationApiTests
{
    /// <summary>
    /// If the middleware component is added to the application builder, an WebApi
    /// is exposed allowing the querying of TypeScript files based on a resource name.
    /// </summary>
    [Fact]
    public Task MiddlewareConfiguresRestApi_ToReadGeneratedCode()
    {
        return WebHostFixture.TestAsync<DocController>(async host =>
        {
            var webResponse = await host
                .ArrangeForRestCodeGen()
                .Act.OnClient(client => client.GetAsync("/api/net-fusion/rest?resource=ResourceOne"));

            await webResponse.Assert.HttpResponseAsync(async response =>
            {
                string code = await response.Content.ReadAsStringAsync();
                code.Should().NotBeNull();
                code.Should().Contain("modelOneProp: string");
            });
        });
    }

    /// <summary>
    /// If a TypeScript code file is queried for an unknown resource name,
    /// a HTTP 404 status code is returned.
    /// </summary>
    [Fact]
    public Task UnknownResourceNameSpecified_ApiReturns404()
    {
        return WebHostFixture.TestAsync<DocController>(async host =>
        {
            var webResponse = await host
                .ArrangeForRestCodeGen()
                .Act.OnClient(client => client.GetAsync("/api/net-fusion/rest?resource=ResourceNine"));

            webResponse.Assert.HttpResponse(response =>
            {
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            });
        });
    }
}