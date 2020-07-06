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
        public Task<IActionResult> TestActionComments() => Task.FromResult<IActionResult>(Ok());
    }
}
