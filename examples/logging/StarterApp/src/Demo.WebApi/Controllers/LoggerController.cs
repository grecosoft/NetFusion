using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class LoggerController : Controller
    {
        private readonly ILogger _logger;

        public LoggerController(ILogger<Test> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult TestLog()
        {
            _logger.LogWarning("Warning Log");
            return Ok();
        }
    }

    public class Test
    {

    }
}
