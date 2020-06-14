using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("metadata/{id}")]
        public IActionResult ActionDetails(int id) => Ok();
        
        //-- Controller actions for testing Route Parameters:
        
        [HttpGet]
        public IActionResult GetResource() => Ok();

        [HttpGet("metadata-scenario-1/{id}/{v1}")]
        public IActionResult RequiredRouteParams(int id, string v1) => Ok();

        [HttpGet("metadata-scenario-2/{id}/{v1?}/{v2?}")]
        public IActionResult OptionalRouteParams(int id, int? v1 = null, string v2 = null) => Ok();

        [HttpGet("metadata-scenario-3/{id}/{v1?}/{v2?}")]
        public IActionResult OptionalRouteParamsWithDefaults(int id, int? v1 = 100, string v2 = "abc") => Ok();

        //-- Controller actions for testing Header Parameters:
        
        [HttpGet("metadata-scenario-10")]
        public IActionResult RequiredHeaderParams(
            [FromHeader(Name = "header-value1")] int value1,
            [FromHeader(Name = "header-value2")] string value2) => Ok();

        [HttpGet("metadata-scenario-11")]
        public IActionResult OptionalHeaderParams(
            [FromHeader(Name = "header-value1")]int? value1 = null,
            [FromHeader(Name = "header-value2")]string value2 = null) => Ok();
        
        [HttpGet("metadata-scenario-12")]
        public IActionResult OptionalHeaderParamsWithDefaults(
            [FromHeader(Name = "header-value1")]int? value1 = 100,
            [FromHeader(Name = "header-value2")]string value2 = "abc") => Ok();
        
        //-- Controller actions for testing Query Parameters:
        
        [HttpGet("metadata-scenario-20")]
        public IActionResult RequiredQueryParams(
            [FromQuery(Name = "query-value1")] int value1,
            [FromQuery(Name = "query-value2")] string value2) => Ok();

        [HttpGet("metadata-scenario-21")]
        public IActionResult OptionalQueryParams(
            [FromQuery(Name = "query-value1")]int? value1 = null,
            [FromQuery(Name = "query-value2")]string value2 = null) => Ok();
        
        [HttpGet("metadata-scenario-22")]
        public IActionResult OptionalQueryParamsWithDefaults(
            [FromQuery(Name = "query-value1")]int? value1 = 100,
            [FromQuery(Name = "query-value2")]string value2 = "abc") => Ok();
        
        //-- Controller actions for testing Responses.
        
        [HttpGet("metadata-scenario-30"),
         ProducesResponseType(typeof(ResponseModel), StatusCodes.Status200OK)]
        public IActionResult ActionWithResponseTypeAndStatus() => Ok();

        [HttpGet("metadata-scenario-31"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ActionWithMultipleStatuses() => Ok();

        [HttpGet("metadata-scenario-32"),
         ProducesResponseType(typeof(ResponseModel), StatusCodes.Status200OK),
         ProducesResponseType(typeof(ResponseModel), StatusCodes.Status418ImATeapot)]
        public IActionResult ActionWithStatusCodesWithSameResponseType() => Ok();
        
        
    }
}