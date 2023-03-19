using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Web.Rest.Docs;

namespace NetFusion.Web.UnitTests.Rest.DocGeneration.Server;

[ApiController]
[Route("api/doc/tests")]
public class DocController : ControllerBase
{
    /// <summary>
    /// This is an example comment for a controller's action method.
    /// </summary>
    /// <returns>Returns a resource.</returns>
    [HttpGet("action/comments")]
    public IActionResult TestActionComments() => Ok();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p1">First parameter comment.</param>
    /// <param name="p2">Second parameter comment.</param>
    /// <returns></returns>
    [HttpGet("action/route-param/{p1}/comments/{p2}")]
    public IActionResult TestActionRouteParamComments(string p1, int p2) => Ok();


    [HttpGet("action/route-param/{p1?}/default")]
    public IActionResult TestActionDefaultRouteParamValue([FromRoute]int? p1 = 100) => Ok();


    [HttpGet("action/multiple/statuses"),
     ProducesResponseType(StatusCodes.Status200OK),
     ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult TestActionWithMultipleResponseStatues() => Ok();


    [HttpGet("action/multiple/response/types"),
     ProducesResponseType(typeof(TestResponseModel), StatusCodes.Status200OK),
     ProducesResponseType(typeof(TestCreatedResponseModel), StatusCodes.Status201Created)]
    public IActionResult TestWithMutilpleResponseTypes() => Ok();


    [HttpGet("action/embedded/resource"),
     ProducesResponseType(typeof(RootResponseModel), StatusCodes.Status200OK),
     EmbeddedResource(typeof(RootResponseModel), typeof(EmbeddedChildModel), "embedded-model")]
    public IActionResult TestEmbeddedResource() => Ok();


    [HttpGet("action/embedded/resource/collection"),
     ProducesResponseType(typeof(RootResponseModel), StatusCodes.Status200OK),
     EmbeddedResource(typeof(RootResponseModel), typeof(EmbeddedChildModel), "embedded-models", isCollection: true)]
    public IActionResult TestEmbeddedResourceCollection() => Ok();


    [HttpGet("action/resource/links"),
     ProducesResponseType(typeof(ModelWithResourceLinks), StatusCodes.Status200OK),
     EmbeddedResource(typeof(ModelWithResourceLinks), typeof(EmbeddedModelWithResourceLinks), "child-with-links")]
    public IActionResult TestResourceWithLinks() => Ok();


    [HttpGet("action/embedded/resource/links"),
     ProducesResponseType(typeof(ModelWithResourceLinks), StatusCodes.Status200OK),
     EmbeddedResource(typeof(ModelWithResourceLinks), typeof(EmbeddedModelWithResourceLinks), "child-with-links")]
    public IActionResult TestEmbeddedResourceWithLinks() => Ok();


    /// <summary>
    /// Returns details for an associated resource.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="versionNumber"></param>
    /// <returns></returns>
    [HttpGet("action/resource/{id}/details/{versionNumber}")]
    public IActionResult GetResourceDetails(string id, int versionNumber) => Ok();


    /// <summary>
    /// Returns details for an embedded associated resource.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("action/embedded/{id}/details")]
    public IActionResult GetEmbeddedResourceDetails(string id) => Ok();


    [HttpGet("action/headers")]
    public IActionResult TestWithHeaders([FromHeader]int id, [FromHeader]string version) => Ok();


    [HttpGet("action/headers/default")]
    public IActionResult TestWithHeaderDefaultValue([FromHeader]string version = "1.0.0") => Ok();


    [HttpGet("action/queries")]
    public IActionResult TestWithQuery([FromQuery] int key, [FromQuery] string unit) => Ok();


    [HttpGet("action/queries/default")]
    public IActionResult TestWithQueryDefaultValue(
        [FromQuery] int? rating, [FromQuery] string version = "9.0.0") => Ok();


    [HttpPost("action/body/post")]
    public IActionResult TestWithPopulatedFromBody([FromBody] TestRequestModel model) => Ok();

}