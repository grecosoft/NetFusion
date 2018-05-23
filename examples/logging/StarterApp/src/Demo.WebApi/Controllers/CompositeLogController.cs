using Microsoft.AspNetCore.Mvc;
using NetFusion.Bootstrap.Container;

namespace Demo.WebApi.Controllers
{
    [Route("api/logs")]
    public class CompositeLogController : Controller
    {
        private readonly IAppContainer _container;

        public CompositeLogController(
            IAppContainer container)
        {
            _container = container;
        }

        [HttpGet("composite")]
        public IActionResult GetLog()
        {
            return Ok(_container.Log);
        }
    }    
}