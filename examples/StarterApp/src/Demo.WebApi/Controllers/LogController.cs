using Microsoft.AspNetCore.Mvc;
using NetFusion.Bootstrap.Container;

namespace Demo.WebApi.Controllers
{
    [Route("api/log")]
    public class LogController : Controller
    {
        private readonly ICompositeApp _compositeApp;
        
        public LogController(ICompositeApp compositeApp)
        {
            _compositeApp = compositeApp;
        }
        
        [HttpGet("composite")]
        public IActionResult GetLog()
        {
            return Ok(_compositeApp.Log);
        }
    }
}
