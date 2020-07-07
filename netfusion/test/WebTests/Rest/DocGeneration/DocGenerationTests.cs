using System.Threading.Tasks;
using WebTests.Hosting;
using WebTests.Rest.DocGeneration.Server;
using Xunit;
using System.Linq;
using FluentAssertions;

namespace WebTests.Rest.DocGeneration
{
    public class DocGenerationTests
    {
        // The comment, assocated with the action method, is specified as the description
        // for the returned ApiActionDoc.
        [Fact]
        public Task DocsForWebApiMethodReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient( client => client.GetAsync("api/doc/tests/action/comments".GetDocUrl()));

                webResponse.Assert.HttpResponse(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.Description.Should().NotBeNullOrWhiteSpace()
                        .And.Should().Be("This is an example comment for a controller's action method.");
                });
            });
        }

        // When a action method has associated route parameters, the comments associated with
        // the parameters are specified on each ApiParameterDoc instance.
        [Fact]
        public Task DocsForEachRouteParamReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/route-param/{p1}/comments/{p2}".GetDocUrl()));

                webResponse.Assert.HttpResponse(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.RouteParams.Should().NotBeNull()
                        .And.Subject.Should().HaveCount(2);

                    var firstParm = actionDoc.RouteParams.ElementAt(0);
                    firstParm.Name.Should().Be("p1");
                    firstParm.Type.Should().Be("String");
                    firstParm.DefaultValue.Should().Be("First parameter comment.");

                    var secondParm = actionDoc.RouteParams.ElementAt(0);
                    secondParm.Name.Should().Be("p2");
                    secondParm.Type.Should().Be("Number");
                    secondParm.DefaultValue.Should().Be("Second parameter comment.");

                });
            });
        }

        [Fact]
        public Task DocForRouteParam_IncludesDefaultValue()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/route-param/{p1}/default".GetDocUrl()));

                webResponse.Assert.HttpResponse(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.RouteParams.Should().NotBeNull()
                        .And.Subject.Should().HaveCount(1);

                    var paramWithDefault = actionDoc.HeaderParams.FirstOrDefault(p => p.Name == "p1");
                    paramWithDefault.Should().NotBeNull();
                    paramWithDefault.IsOptional.Should().BeTrue();
                    paramWithDefault.DefaultValue.Should().Be(100);
                });
            });
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
