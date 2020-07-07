using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebTests.Rest.DocGeneration.Server
{
    [ApiController]
    [Route("api/doc/tests")]
    public class DocController : ControllerBase
    {
        /// <summary>
        /// This is an example comment for a controller's action method.
        /// </summary>
        /// <returns></returns>
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

        [HttpGet("action/route-param/{p1?}")]
        public IActionResult TestActionDefaultRouteParamValue(int? p1 = 100) => Ok();

        
    }
}
