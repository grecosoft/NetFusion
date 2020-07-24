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
        // The comment, associated with the action method, is specified as the description
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
                    actionDoc.Description.Should().Be("This is an example comment for a controller's action method. Returns a resource.");
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

                    var firstParam = actionDoc.RouteParams.ElementAt(0);
                    firstParam.Name.Should().Be("p1");
                    firstParam.Type.Should().Be("String");
                    firstParam.Description.Should().Be("First parameter comment.");

                    var secondParam = actionDoc.RouteParams.ElementAt(1);
                    secondParam.Name.Should().Be("p2");
                    secondParam.Type.Should().Be("Number");
                    secondParam.Description.Should().Be("Second parameter comment.");
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

                    var paramWithDefault = actionDoc.RouteParams.First(p => p.Name == "p1");
                    paramWithDefault.IsOptional.Should().BeTrue();
                    paramWithDefault.DefaultValue.Should().Be("100");
                });
            });
        }

        // The API documentation contains the headers accepted by the REST method.
        [Fact]
        public Task DocsForEachHeaderParamReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/headers".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.HeaderParams.Should().NotBeEmpty();
                    actionDoc.HeaderParams.Should().HaveCount(2);

                    var firstHeaderDoc = actionDoc.HeaderParams.First(p => p.Name == "id");
                    firstHeaderDoc.Type.Should().Be("Number");
                    firstHeaderDoc.IsOptional.Should().BeFalse();

                    var secondHeaderDoc = actionDoc.HeaderParams.First(p => p.Name == "version");
                    secondHeaderDoc.Type.Should().Be("String");
                    secondHeaderDoc.IsOptional.Should().BeFalse();

                });
            });
        }

        // The API header documentation indicates if the header value has a default
        // value that will be used if not specified.
        [Fact]
        public Task DocForEachHeaderParam_IncludesDefaultValue()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/headers/default".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.HeaderParams.Should().NotBeEmpty();
                    actionDoc.HeaderParams.Should().HaveCount(1);

                    var headerParamDoc = actionDoc.HeaderParams.First(p => p.Name == "version");
                    headerParamDoc.Type.Should().Be("String");
                    headerParamDoc.IsOptional.Should().BeTrue();
                    headerParamDoc.DefaultValue.Should().Be("1.0.0");
                });
            });
        }

        // The API documentation contains the query parameters accepted by the REST method.
        [Fact]
        public Task DocsForEachQueryParamReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/queries".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.QueryParams.Should().NotBeEmpty();
                    actionDoc.QueryParams.Should().HaveCount(2);

                    var firstQueryDoc = actionDoc.QueryParams.First(p => p.Name == "key");
                    firstQueryDoc.Type.Should().Be("Number");
                    firstQueryDoc.IsOptional.Should().BeFalse();

                    var secondQueryDoc = actionDoc.QueryParams.First(p => p.Name == "unit");
                    secondQueryDoc.Type.Should().Be("String");
                    secondQueryDoc.IsOptional.Should().BeFalse();

                });
            });
        }

        // The API query parameter documentation indicates if the query value has
        // a default value that will be used if not specified.
        [Fact]
        public Task DocForEachQueryParam_IncludesDefaultValue()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/queries/default".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();

                    actionDoc.QueryParams.Should().NotBeEmpty();
    
                    var firstQueryDoc = actionDoc.QueryParams.First(p => p.Name == "version");
                    firstQueryDoc.Type.Should().Be("String");
                    firstQueryDoc.IsOptional.Should().BeTrue();
                    firstQueryDoc.DefaultValue.Should().Be("9.0.0");

                    var secondQueryDoc = actionDoc.QueryParams.First(p => p.Name == "rating");
                    secondQueryDoc.Type.Should().Be("Number");
                    firstQueryDoc.IsOptional.Should().BeTrue();
                });
            });
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
            typeof(ModelWithExposedName).GetResourceName()
                .Should().Be("api.sample.model");
        }

        // If a response model does not explicitly specify an external type name,
        // the class name including the namespace is used.  The prior approach is
        // best since the name is hard-coded and will not change if the internal
        // class name is modified.
        [Fact]
        public void ApiModelTypeAndNamespace_IfNoExposedName()
        {
            typeof(ModelWithoutExposedName).GetResourceName()
                .Should().Be(nameof(ModelWithoutExposedName));
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
                        .First(rd => rd.ResourceDoc.ResourceName == typeof(TestResponseModel).GetResourceName());

                    firstRespDoc.Status.Should().Be(StatusCodes.Status200OK);

                    var secondRespDoc = actionDoc.ResponseDocs
                        .First(rd => rd.ResourceDoc.ResourceName == typeof(TestCreatedResponseModel).GetResourceName());

                    secondRespDoc.Status.Should().Be(StatusCodes.Status201Created);
                });
            });
        }

        // An API can specify which resources will be embedded within a parent resource.  When a resource
        // has an embedded resource, the resource document will have its embedded-resources collection populated.
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

                    var responseDoc = actionDoc.ResponseDocs.First();
                    responseDoc.ResourceDoc.ResourceName.Should().Be(typeof(RootResponseModel).GetResourceName());
                    responseDoc.ResourceDoc.EmbeddedResourceDocs.Should().HaveCount(1);

                    var embeddedResDoc = responseDoc.ResourceDoc.EmbeddedResourceDocs.First();
                    embeddedResDoc.EmbeddedName.Should().Be("embedded-model");
                    embeddedResDoc.IsCollection.Should().BeFalse();
                    embeddedResDoc.ResourceDoc.Should().NotBeNull();
                    embeddedResDoc.ResourceDoc.ResourceName.Should().Be(typeof(EmbeddedChildModel).GetResourceName());
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

                    var embeddedResDoc = responseDoc.ResourceDoc.EmbeddedResourceDocs.First();
                    embeddedResDoc.EmbeddedName.Should().Be("embedded-models");
                    embeddedResDoc.IsCollection.Should().BeTrue();
                    embeddedResDoc.ResourceDoc.Should().NotBeNull();
                    embeddedResDoc.ResourceDoc.ResourceName.Should().Be(typeof(EmbeddedChildModel).GetResourceName());

                });
            });
        }

        // When the documentation for a resource is built, a check is made to determine if the resource has
        // any associated linked relations.  If so, documentation is returned for each relation.
        [Fact]
        public Task DocsForResourceRelationsReturned()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/resource/links".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();
                    actionDoc.ResponseDocs.Should().NotBeNullOrEmpty();
                    actionDoc.ResponseDocs.Should().HaveCount(1);

                    var responseDoc = actionDoc.ResponseDocs.First().ResourceDoc;
                    responseDoc.Should().NotBeNull();
                    responseDoc.RelationDocs.Should().NotBeNullOrEmpty();
                    responseDoc.RelationDocs.Should().HaveCount(1);

                    var relationDoc = responseDoc.RelationDocs.First();
                    relationDoc.Name.Should().Be("relation-1");
                    relationDoc.HRef.Should().Be("api/doc/tests/action/resource/{id}/details/{versionNumber}");
                    relationDoc.Description.Should().Be("Returns details for an associated resource.");
                });
            });
        }

        // Resources can be embedded to any nested level.  To keep APIs simple, it is best to limit the depth.
        // Nevertheless, the following validates that a resource within an embedded collection containing another
        // embedded single resource is correctly documented.
        [Fact]
        public Task DocsForEmbeddedResources_RecursivelySet()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/embedded/resource/links".GetDocUrl()));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();
                    var embeddedRelationDoc = actionDoc.ResponseDocs.First()
                        .ResourceDoc
                        .EmbeddedResourceDocs
                        .First()
                        .ResourceDoc
                        .RelationDocs
                        .First();

                    embeddedRelationDoc.Should().NotBeNull();

                    embeddedRelationDoc.Name.Should().Be("relation-2");
                    embeddedRelationDoc.HRef.Should().Be("api/doc/tests/action/embedded/{id}/details");
                    embeddedRelationDoc.Description.Should().Be("Returns details for an embedded associated resource.");
                });
            });
        }

        [Fact] 
        public Task DocsForActionParamPopulatedFormBody()
        {
            return WebHostFixture.TestAsync<DocController>(async host =>
            {
                var webResponse = await host
                    .ArrangeForRestDocs()
                    .Act.OnClient(client => client.GetAsync("api/doc/tests/action/body/post".GetDocUrl("post")));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    var actionDoc = await response.AsApiActionDocAsync();
                    actionDoc.BodyParams.Should().HaveCount(1);
                    actionDoc.BodyParams.First().ResourceDoc.Should().NotBeNull();
                    actionDoc.BodyParams.First().ResourceDoc.Description.Should().Be("Example mode populated from the body of a request.");
                });
            });
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
