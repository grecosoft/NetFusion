using System.Threading.Tasks;
using WebTests.Hosting;
using WebTests.Rest.DocGeneration.Server;
using Xunit;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Resources;

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

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.Description.Should().NotBeNullOrWhiteSpace();
                    actionDoc.Description.Should().Be("This is an example comment for a controller's action method.");
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

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.RouteParams.Should().NotBeNull()
                        .And.Subject.Should().HaveCount(2);

                    var firstParm = actionDoc.RouteParams.ElementAt(0);
                    firstParm.Name.Should().Be("p1");
                    firstParm.Type.Should().Be("String");
                    firstParm.Description.Should().Be("First parameter comment.");

                    var secondParm = actionDoc.RouteParams.ElementAt(1);
                    secondParm.Name.Should().Be("p2");
                    secondParm.Type.Should().Be("Number");
                    secondParm.Description.Should().Be("Second parameter comment.");
                });
            });
        }

        // If a route parameter is optional and a default value is specified, the returned
        // parameter document will include the default value.
        [Fact]
        public Task DocForRouteParam_IncludesDefaultValue()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/route-param/{p1}/default".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.RouteParams.Should().NotBeNull()
                        .And.Subject.Should().HaveCount(1);

                    var paramWithDefault = actionDoc.RouteParams.FirstOrDefault(p => p.Name == "p1");
                    paramWithDefault.Should().NotBeNull();
                    paramWithDefault.IsOptional.Should().BeTrue();
                    paramWithDefault.DefaultValue.Should().Be("100");
                });
            });
        }

        // The API documentation contains the headers accepted by the REST method.
        [Fact]
        public void DocsForEachHeaderParamReturned()
        {

        }

        // The API header documentation indicates if the header value has a default
        // value that will be used if not specified.
        [Fact]
        public void DocForEachHeaderParam_IncludesDefaultValue()
        {

        }

        // The API header documentation indicates if the header is optional.
        [Fact]
        public void DocForEachHeaderParam_IncludesIfOptional()
        {

        }

        // The API documentation contains the query parameters accepted by the REST method.
        [Fact]
        public void DocsForEachQueryParamReturned()
        {

        }

        // The API query parameter documentation indicates if the query value has
        // a default value that will be used if not specified.
        [Fact]
        public void DocForEachQueryParam_IncludesDefaultValue()
        {

        }

        // The API query parameter documentation inidcates if the query is options.
        [Fact]
        public void DocForEachQueryParam_IncludesIfOptional()
        {

        }

        // The response documentation will include the possible status types
        // returned from the WebApi action method.
        [Fact]
        public Task DocsForEachPossibleStatusCodeReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/multiple/statuses".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.ResponseDocs.Should().HaveCount(2);
                    actionDoc.ResponseDocs.Where(r => r.Status == StatusCodes.Status200OK).Should().HaveCount(1);
                    actionDoc.ResponseDocs.Where(r => r.Status == StatusCodes.Status201Created).Should().HaveCount(1);
                });
            });
        }

        // Response models returned from an API can specify a name used to identity
        // the response type to external consumers.   
        [Fact]
        public void ApiModelType_CanSpecifyExposedName()
        {
            typeof(ModelWithExposedName).GetExposedResourceName()
                .Should().Be("api.sample.model");
        }

        // If a response model does not explicitly specify an external type name,
        // the class name including the namespace is used.  The prior approach is
        // best since the name is hard-coded and will not change if the internal
        // class name is modified.
        [Fact]
        public void ApiModelTypeAndNamespace_IfNoExposedName()
        {
            typeof(ModelWithoutExposedName).GetExposedResourceName()
                .Should().Be(typeof(ModelWithoutExposedName).FullName);
        }

        // If an API method specifies the response type and status code, both
        // will be contained within the response document.
        [Fact]
        public Task DocsForEachResponseResourceReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/multiple/response/types".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();
                    actionDoc.ResponseDocs.Should().HaveCount(2);

                    var firstRespDoc = actionDoc.ResponseDocs
                        .FirstOrDefault(rd => rd.ResourceDoc.ResourceName == typeof(TestResponseModel).GetExposedResourceName());

                    firstRespDoc.Status.Should().Be(StatusCodes.Status200OK);

                    var secondRespDoc = actionDoc.ResponseDocs
                        .FirstOrDefault(rd => rd.ResourceDoc.ResourceName == typeof(TestCreatedResponseModel).GetExposedResourceName());

                    secondRespDoc.Status.Should().Be(StatusCodes.Status201Created);
                });
            });
        }

        // An API can specify which resources will be embeeded within a parent resource.  When a resource
        // has an embeddedresource, the resource document will have its embedded-resources collection populated.
        [Fact]
        public Task DocsForEmbeddedResourceReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/embedded/resource".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    var responseDoc = actionDoc.ResponseDocs.FirstOrDefault();
                    responseDoc.Should().NotBeNull();
                    responseDoc.ResourceDoc.ResourceName.Should().Be(typeof(RootResponseModel).GetExposedResourceName());
                    responseDoc.ResourceDoc.EmbeddedResources.Should().HaveCount(1);

                    var embeddedResDoc = responseDoc.ResourceDoc.EmbeddedResources.FirstOrDefault();
                    embeddedResDoc.Should().NotBeNull();
                    embeddedResDoc.EmbeddedName.Should().Be("embedded-model");
                    embeddedResDoc.IsCollection.Should().BeFalse();
                    embeddedResDoc.ResourceDoc.Should().NotBeNull();
                    embeddedResDoc.ResourceDoc.ResourceName.Should().Be(typeof(EmbeddedChildModel).GetExposedResourceName());
                });
            });
        }

        // An API can specify which resources will have an embedded collection of resources.  When a collection of
        // resources is embedded, the IsCollection property will be set to true.
        [Fact]
        public Task DocsForEmbeddedResourceCollectionReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/embedded/resource/collection".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();
                    var responseDoc = actionDoc.ResponseDocs.First();

                    var embeddedResDoc = responseDoc.ResourceDoc.EmbeddedResources.FirstOrDefault();
                    embeddedResDoc.Should().NotBeNull();
                    embeddedResDoc.EmbeddedName.Should().Be("embedded-models");
                    embeddedResDoc.IsCollection.Should().BeTrue();
                    embeddedResDoc.ResourceDoc.Should().NotBeNull();
                    embeddedResDoc.ResourceDoc.ResourceName.Should().Be(typeof(EmbeddedChildModel).GetExposedResourceName());

                });
            });
        }

        // Resources can be embedded to any nested level.  To keep APIs simple, it is best to limit the depth.
        // Nevertheless, the following validates that a resource within an embedded collection containing another
        // embedded single resource is correctly documented.
        [Fact]
        public void DocsForEmbeddedResources_RecursivelySet()
        {

        }

        // When the documentation for a resource is built, a check is made to determine if the resource has
        // any associated linked relations.  If so, documentation is returned for each relation.
        [Fact]
        public void DocsForResourceRelationsReturned()
        {

        }

        // If an API Web Url is specified for which the documentation could
        // not be determined, an HTTP 404 is returned.
        [Fact]
        public Task IfNoApiDocumentFound_Http404Returned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/covid/19/sucks".GetDocUrl()));

                webResponse.Assert.HttpResponse(response =>
                {
                    response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
                });
            });
        }
    }
}
