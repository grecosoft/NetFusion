using System;
using System.Threading.Tasks;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.ApiMetadata.Setup;
using Xunit;

namespace WebTests.Rest.ApiMetadata
{
    /// <summary>
    /// Validates the population of route parameter metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class RouteParameterTests
    {
        [Fact]
        public Task RequiredRouteParams()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "RequiredRouteParams", typeof(int), typeof(string));
            
                actionMetadata.RouteParameters.AssertParamMeta<int>("id");
                actionMetadata.RouteParameters.AssertParamMeta<string>("v1");
            });
        }

        [Fact]
        public Task OptionalRouteParams()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "OptionalRouteParams", typeof(int), typeof(int?), typeof(string));
                
                actionMetadata.RouteParameters.AssertParamMeta<int>("id");
                actionMetadata.RouteParameters.AssertParamMeta<int?>("v1", true);
                actionMetadata.RouteParameters.AssertParamMeta<string>("v2", true);
            });
        }
            
        [Fact]
        public Task OptionalRouteParamsWithDefaults()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "OptionalRouteParamsWithDefaults", typeof(int), typeof(int?), typeof(string));
                
                actionMetadata.RouteParameters.AssertParamMeta<int>("id");
                actionMetadata.RouteParameters.AssertParamMeta<int?>("v1", true, 100);
                actionMetadata.RouteParameters.AssertParamMeta<string>("v2", true, "abc");
            });
        }

        [Fact]
        public Task RouteParamPopulatedFromBodyPost()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "RouteParamWithPostBody", typeof(int), typeof(MetaBodyPost));
            
                actionMetadata.RouteParameters.AssertParamMeta<int>("id");
                actionMetadata.BodyParameters.AssertParamMeta<MetaBodyPost>("data");
            });
        }

        [Fact]
        public Task ActionParamsPopulatedFromObjectProperties()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "ActionObjectPropertySources", typeof(int), typeof(HeaderParamSource), typeof(QueryParamSource));
            
                actionMetadata.RouteParameters.AssertParamMeta<int>("id");
                actionMetadata.QueryParameters.AssertParamMeta<string>("Filter");
                actionMetadata.QueryParameters.AssertParamMeta<string>("Version");
                actionMetadata.HeaderParameters.AssertParamMeta<string>("ClientId");
                actionMetadata.HeaderParameters.AssertParamMeta<DateTime>("AsOfDate");
            });
        }
    }
}