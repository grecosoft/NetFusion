using Microsoft.AspNetCore.Mvc;
using NetFusion.Bootstrap.Container;

namespace Demo.WebApi.Controllers
{
    [Route("api/log")]
    public class LogController : Controller
    {
        [HttpGet("composite")]
        public IActionResult GetLog()
        {
            return Ok(CompositeContainer.Instance.Log);
        }
    }
}
