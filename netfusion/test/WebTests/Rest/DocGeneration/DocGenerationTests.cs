using System.Threading.Tasks;
using WebTests.Hosting;
using WebTests.Rest.DocGeneration.Server;
using Xunit;
using FluentAssertions;

namespace WebTests.Rest.DocGeneration
{
    public class DocGenerationTests
    {
        [Fact]
        public Task DocsForWebApiMethodReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient( client => client.GetAsync("/api/net-fusion/rest?doc=api/doc/tests/action/comments"));

                webResponse.Assert.HttpResponse(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.Description.Should().NotBeNullOrWhiteSpace()
                        .And.Should().Be("This is an example comment for a controller's action method.");
                });
            });
        }

        [Fact]
        public void DocsForEachRouteParamReturned()
        {

        }

        [Fact]
        public void DocsForEachHeaderParamReturned()
        {

        }

        [Fact]
        public void DocsForEachQueryParamReturned()
        {

        }

        [Fact]
        public void DocsForEachPossibleStatusCodeReturned()
        {

        }

        [Fact]
        public void DocsForEachResponseResourceReturned()
        {

        }

        [Fact]
        public void DocsForEmbeddedResourcesReturned()
        {

        }

        [Fact]
        public void DocsForResourceRelationsReturned()
        {

        }

        [Fact]
        public void IfNowDocumentFound_Http404Returned()
        {

        }

    }
}
