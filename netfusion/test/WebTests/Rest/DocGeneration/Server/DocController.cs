using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Docs;

namespace WebTests.Rest.DocGeneration.Server
{
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
    }
}
