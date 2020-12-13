using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Settings;
using WebTests.Hosting;
using WebTests.Rest.LinkGeneration.Client;
using WebTests.Rest.LinkGeneration.Server;
using WebTests.Rest.Setup;
using Xunit;


#pragma warning disable 1570
namespace WebTests.Rest.LinkGeneration
{
    /// <summary>
    /// Unit-tests 
    /// </summary>
    public class LinkGenerationTests
    {        
        /// <summary>
        /// The following tests the selection of resource links using type safe expressions to specify
        /// the controller and action method to call for a given resource URL.  This is the approach that
        /// should be use most often.  This allows resource mappings to specify the intent of the link and
        /// allows the MVC routing to determine the URL based on the current configuration.
        /// 
        /// Scenarios:
        ///     Scenario 1: Controller Action with single required route parameter.
        ///     
        ///     Scenario 2: Controller Action with two required route parameters.
        ///     
        ///     Scenario 3: Controller Action with one required and one optional route parameter.  Testing
        ///         when resource property has value for optional parameter and when the resource property
        ///         does not have a specific value.
        ///         
        ///     Scenario 4: Controller Action with multiple optional parameters.
        ///     
        ///     Scenario 5: Controller Action with no route parameter with one parameter sourced from posted data.
        ///         
        ///     Scenario 6: Controller Action with single route sourced parameter and additional parameter
        ///         sourced from the posted data.
        ///     
        /// </summary>
        /// <example>
        /// 
        ///     .Map<LinkedResource>()
        ///         .LinkMeta<LinkedResourceController>(
        ///             meta => meta.Url("scenario-1", (c, r) => c.GetById(r.IdValue)))
        ///
        ///         .LinkMeta<LinkedResourceController>(
        ///             meta => meta.Url("scenario-2", (c, r) => c.GetByIdAndRequiredParam(r.IdValue, r.Value2)));
        ///             
        ///         ...
        ///             
        /// </example>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_AllRouteParamsSupplied()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                     var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                     // Required Route Parameters:
                     apiResponse.Resource.AssertLink("scenario-1", HttpMethod.Get, "/api/linked/resource/scenario-1/10");
                     apiResponse.Resource.AssertLink("scenario-2", HttpMethod.Get, "/api/linked/resource/scenario-2/10/param-one/value-2");
                });
            });
        }
        
        /// <summary>
        /// Tests the scenario where the route has an option parameter where the
        /// corresponding model property contains a value.
        /// </summary>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithOptionalSuppliedRouteParam()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    // Optional Route Parameter with supplied value:
                    apiResponse.Resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one/300");
                });
            });
        }
        
        /// <summary>
        /// Tests the scenario where the route has an optional parameter where the
        /// corresponding model property is null.  In this case, the optional parameter
        /// is not contained in the generated URL.
        /// </summary>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithOptionalNotSuppliedRouteParam()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = null,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    // Optional Route Parameter with supplied value:
                    apiResponse.Resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one");
                });
            });
        }
        
        /// <summary>
        /// Tests the scenario where the route has multiple route parameters where the
        /// model being returned has value for all corresponding parameters.
        /// </summary>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithMultipleOptionalSuppliedRouteParams()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 600,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    // Optional Route Parameter with supplied value:
                    apiResponse.Resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one/600/value-2");
                });
            });
        }
        
        /// <summary>
        /// Tests the scenario where the route has motile route parameters where the
        /// model being returned has null values for each corresponding parameter.
        /// </summary>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithMultipleOptionalNotSuppliedRouteParams()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = null,
                Value3 = null,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    // Optional Route Parameter with supplied value:
                    apiResponse.Resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one");
                });
            });
        }
        
        /// <summary>
        /// Tests the scenario where the route does not have any specified parameters but the
        /// action method has a parameter being populated from the request message body.
        /// </summary>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_NoRouteParamsWithPostedData()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 600,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    // Optional Route Parameter with supplied value:
                    apiResponse.Resource.AssertLink("scenario-5", HttpMethod.Post, "/api/linked/resource/scenario-5/create");
                });
            });
        }
        
        /// <summary>
        /// Tests the scenario where the route has a parameter in addition to an action
        /// parameter populated from the body of the request.
        /// </summary>
        [Fact]
        public Task CanGenerateUrl_ActionExpression_RouteParamWithPostedData()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 600,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    // Optional Route Parameter with supplied value:
                    apiResponse.Resource.AssertLink("scenario-6", HttpMethod.Put, "/api/linked/resource/scenario-6/10/update");
                });
            });
        }
        

        /// <summary>
        /// This unit test validates that a resource mapping can specify a URL as a hard-coded string.  This 
        /// approach should be used the least often when specifying resource links that call controller action
        /// methods defined within the boundaries of the same WebApi.  However, this approach can be used when
        /// adding resource links that invoke services provided by another WebApi.
        /// </summary>
        /// <example>
        /// 
        /// .Map<LinkedResource>()
        ///     .LinkMeta<LinkedResourceController>(
        ///         meta => meta.Href("scenario-20", HttpMethod.Options, "http://external/api/call/info"));
        ///         
        /// </example>
        [Fact]
        public Task CanGenerateUrl_FromHardCodedString()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    apiResponse.Resource.AssertLink("scenario-20", HttpMethod.Options, "http://external/api/call/info");
                });
            });
        }

        /// <summary>
        /// This unit test validates a resource link mapping that is specified as a hard-coded string but also
        /// contains values corresponding to properties of the resource.  Like the hard-coded scenario, this 
        /// approach should be used when calling external API services.  
        /// 
        /// Unlike the hard-coded string approach, this approach allows you to specify resource properties within 
        /// the generated URL using string interpolation providing compile-time type checking.
        /// </summary>
        /// <example>
        /// 
        /// .Map<LinkedResource>()
        ///      .LinkMeta<LinkedResourceController>(
        ///         meta => meta.Href("scenario-25", HttpMethod.Options, r => $"http://external/api/call/{r.Id}/info/{r.Value2}"));
        ///         
        /// </example>
        [Fact]
        public Task CanGenerateUrl_FromStringInterpolatedResourceUrl()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    apiResponse.Resource.AssertLink("scenario-25", HttpMethod.Options, "http://external/api/call/10/info/value-2");
                });
            });
        }

        /// <summary>
        /// When specifying resource links within a resource mapping, additional optional metadata can be specified:
        ///     - Name
        ///     - Title
        ///     - Type
        ///     - HrefLang
        /// </summary>
        [Fact]
        public Task ResourceMap_CanSpecify_AdditionalOptionalLinkProperties()
        {
            var mockResource = new StateModel
            {
                Id = 10,
                Value2 = "value-2"
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    apiResponse.Resource.AssertLink("scenario-30", HttpMethod.Options, "http://external/api/call/10/info/value-2");

                    var link = apiResponse.Resource.Links["scenario-30"];
                    Assert.Equal("test-title", link.Title);
                    Assert.Equal("test-type", link.Type);
                    Assert.Equal("test-href-lang", link.HrefLang);
                });
            });
        } 
        
        /// <summary>
        /// Where all prior examples had route parameter values specified, this scenario tests
        /// the case where the returned URL is template based.  In this case, the returned URL
        /// does not have the route parameters populated from the model properties.  Template
        /// based URLs are mostly used on entry resources specifying initial URLs that can be
        /// called to start communication with the WebApi.  Once a resource is returned, the
        /// majority of the returned URLs will be complete by having the route parameters set
        /// from properties on the returned resource's model.
        /// </summary>
        [Fact]
        public Task ResourceMap_CanSpecify_TemplateUrls()
        {
            var mockResource = new StateModel
            {
                Id = 10,
                Value2 = "value-2"
            };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                     
                    apiResponse.Resource.AssertLink("scenario-31", HttpMethod.Post, "api/linked/resource/scenario-33/{id}/comment");
                });
            });
        }

        /// <summary>
        /// The client can specify the embed query parameter to specify the names
        /// of the embedded resources to return. 
        /// </summary>
        [Fact]
        public Task Client_CanSpecify_EmbeddedResources()
        {
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var mockResource = new StateModel();
                
                var webResponse = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnClient(async client => await client.GetAsync("api/linked/resource/embedded?embed=a,b,c"));

                await webResponse.Assert.HttpResponseAsync(async response =>
                {
                    string result = await response.Content.ReadAsStringAsync();
                    result.Should().Be("a|b|c");
                });
            });
        }

        /// <summary>
        /// Setting of links are recursive.  If a parent resource contains embedded
        /// resources, each embedded resource will have links populated.
        /// </summary>
        [Fact]
        public Task Links_AreSet_OnEmbeddedResources()
        {
            var mockModel = new StateModel
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };

            var mockEmbeddedModel = new StateEmbeddedModel { Id = 10 };
            
            return WebHostFixture.TestAsync<ResourceController>(async host =>
            {
                var webResponse = await host
                    .ArrangeWithDefaults(mockModel, mockEmbeddedModel)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource/embedded/child", HttpMethod.Get);
                        request.UseHalDefaults();
                        return await client.SendForHalAsync<ClientModelStub>(request);
                    });

                webResponse.Assert.ApiResponse(response =>
                {
                    var apiResponse = (ApiHalResponse<ClientModelStub>)response;
                    var embeddedChild = apiResponse.Resource.GetEmbeddedResource<ClientModelStub>("embedded-resource");

                    embeddedChild.Should().NotBeNull();
                    embeddedChild.AssertLink("embedded-child", HttpMethod.Get, "http://test/resource/10");
                });
            });
        }
        
    }
}
