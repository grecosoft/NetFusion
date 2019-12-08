﻿using System.Net.Http;
using System.Threading.Tasks;
using NetFusion.Rest.Client;
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
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                     var resource = (LinkedResourceModel)apiResponse.Content;
                     
                     // Required Route Parameters:
                     resource.AssertLink("scenario-1", HttpMethod.Get, "/api/linked/resource/scenario-1/10");
                     resource.AssertLink("scenario-2", HttpMethod.Get, "/api/linked/resource/scenario-2/10/param-one/value-2");
                });
            });
        }
        
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithOptionalSuppliedRouteParam()
        {
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    // Optional Route Parameter with supplied value:
                    resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one/300");
                });
            });
        }
        
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithOptionalNotSuppliedRouteParam()
        {
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = null,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    // Optional Route Parameter with supplied value:
                    resource.AssertLink("scenario-3", HttpMethod.Get, "/api/linked/resource/scenario-3/10/param-one");
                });
            });
        }
        
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithMultipleOptionalSuppliedRouteParams()
        {
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 600,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    // Optional Route Parameter with supplied value:
                    resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one/600/value-2");
                });
            });
        }
        
        [Fact]
        public Task CanGenerateUrl_ActionExpression_WithMultipleOptionalNotSuppliedRouteParams()
        {
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = null,
                Value3 = null,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    // Optional Route Parameter with supplied value:
                    resource.AssertLink("scenario-4", HttpMethod.Get, "/api/linked/resource/scenario-4/10/param-one");
                });
            });
        }
        
        [Fact]
        public Task CanGenerateUrl_ActionExpression_NoRouteParamsWithPostedData()
        {
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 600,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    // Optional Route Parameter with supplied value:
                    resource.AssertLink("scenario-5", HttpMethod.Post, "/api/linked/resource/scenario-5/create");
                });
            });
        }
        
        [Fact]
        public Task CanGenerateUrl_ActionExpression_RouteParamWithPostedData()
        {
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 600,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    // Optional Route Parameter with supplied value:
                    resource.AssertLink("scenario-6", HttpMethod.Put, "/api/linked/resource/scenario-6/10/update");
                });
            });
        }
        

        /// <summary>
        /// This unit test validates that a resource mapping can specify a URL as a hard-coded string.  This 
        /// approach should be used the least often when specifying resource links that call controller action
        /// methods defined within the boundaries of the same application.  However, this approach can be used
        /// when adding resource links that invoke services provided by other applications.
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
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    resource.AssertLink("scenario-20", HttpMethod.Options, "http://external/api/call/info");
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
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value1 = 100,
                Value2 = "value-2",
                Value3 = 300,
                Value4 = 400
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    resource.AssertLink("scenario-25", HttpMethod.Options, "http://external/api/call/10/info/value-2");
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
            var mockResource = new LinkedResource
            {
                Id = 10,
                Value2 = "value-2"
            };
            
            return WebHostFixture.TestAsync<LinkGenerationTests>(async host =>
            {
                var response = await host
                    .ArrangeWithDefaults(mockResource)
                    .Act.OnRestClient(async client =>
                    {             
                        var request = ApiRequest.Create("api/linked/resource", HttpMethod.Get);
                        return await client.SendAsync<LinkedResourceModel>(request);
                    });

                response.Assert.ApiResponse(apiResponse =>
                {
                    var resource = (LinkedResourceModel)apiResponse.Content;
                     
                    resource.AssertLink("scenario-30", HttpMethod.Options, "http://external/api/call/10/info/value-2");

                    var link = resource.Links["scenario-30"];
                    Assert.Equal("test-name", link.Name);
                    Assert.Equal("test-title", link.Title);
                    Assert.Equal("test-type", link.Type);
                    Assert.Equal("test-href-lang", link.HrefLang);
                });
            });
        } 
    }
}
