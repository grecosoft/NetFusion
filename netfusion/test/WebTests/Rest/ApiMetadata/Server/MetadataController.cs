using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources.Hal;
using WebTests.Mocks;

namespace WebTests.Rest.ApiMetadata.Server
{
    [ApiController, Route("api/documented/actions")]
    public class MetadataController : ControllerBase
    {
        private readonly IMockedService _mockedService;
        
        public MetadataController(IMockedService mockedService)
        {
            _mockedService = mockedService;
        }

        [HttpGet]
        public HalResource GetResource() => null;

        [HttpGet("metadata-scenario-1/{id}/{v1}")]
        public HalResource RequiredRouteParams(int id, string v1) => null;

        [HttpGet("metadata-scenario-2/{id}/{v1?}/{v2?}")]
        public HalResource OptionalRouteParams(int id, int? v1 = null, string v2 = null) => null;

        [HttpGet("metadata-scenario-3/{id}/{v1?}/{v2?}")]
        public HalResource OptionalRouteParamsWithDefaults(int id, int? v1 = 100, string v2 = "abc") => null;

        [HttpGet("metadata-scenario-4")]
        public HalResource RequiredHeaderParams([FromHeader(Name = "header-value")] int value) => null;

    }
}