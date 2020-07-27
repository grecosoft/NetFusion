using System.Threading.Tasks;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.ApiMetadata.Setup;
using Xunit;

namespace WebTests.Rest.ApiMetadata
{
    /// <summary>
    /// Validates the population of query parameter metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class QueryParameterTests
    {
        [Fact]
        public Task RequiredQueryParams()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "RequiredQueryParams", typeof(int), typeof(string));

                actionMetadata.QueryParameters.AssertParamMeta<int>("query-value1");
                actionMetadata.QueryParameters.AssertParamMeta<string>("query-value2");
            });
        }
        
        [Fact]
        public Task OptionalQueryParams()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "OptionalQueryParams", typeof(int), typeof(string));

                actionMetadata.QueryParameters.AssertParamMeta<int?>("query-value1", true);
                actionMetadata.QueryParameters.AssertParamMeta<string>("query-value2", true);
            });
        }
        
        [Fact]
        public Task OptionalQueryParamsWithDefaults()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "OptionalQueryParamsWithDefaults", typeof(int), typeof(string));

                actionMetadata.QueryParameters.AssertParamMeta<int?>("query-value1", true, 100);
                actionMetadata.QueryParameters.AssertParamMeta<string>("query-value2", true, "abc");
            });
        }
    }
}