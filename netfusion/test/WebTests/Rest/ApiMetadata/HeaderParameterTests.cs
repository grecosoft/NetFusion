using System.Threading.Tasks;
using WebTests.Rest.ApiMetadata.Server;
using WebTests.Rest.ApiMetadata.Setup;
using Xunit;

namespace WebTests.Rest.ApiMetadata
{
    /// <summary>
    /// Validates the population of header parameter metadata from the underlying
    /// parameter descriptions discovered by the ASP.NET runtime.
    /// </summary>
    public class HeaderParameterTests
    {
        [Fact]
        public Task RequiredHeaderParams()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "RequiredHeaderParams", typeof(int), typeof(string));

                actionMetadata.HeaderParameters.AssertParamMeta<int>("header-value1");
                actionMetadata.HeaderParameters.AssertParamMeta<string>("header-value2");
            });
        }
        
        [Fact]
        public Task OptionalHeaderParams()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "OptionalHeaderParams", typeof(int), typeof(string));

                actionMetadata.HeaderParameters.AssertParamMeta<int?>("header-value1", true);
                actionMetadata.HeaderParameters.AssertParamMeta<string>("header-value2", true);
            });
        }
        
        [Fact]
        public Task OptionalHeaderParamsWithDefaults()
        {
            return TestWebHostSetup.Run(metadata =>
            {
                var actionMetadata = metadata.GetActionMeta<MetadataController>(
                    "OptionalHeaderParamsWithDefaults", typeof(int), typeof(string));

                actionMetadata.HeaderParameters.AssertParamMeta<int?>("header-value1", true, 100);
                actionMetadata.HeaderParameters.AssertParamMeta<string>("header-value2", true, "abc");
            });
        }
    }
}