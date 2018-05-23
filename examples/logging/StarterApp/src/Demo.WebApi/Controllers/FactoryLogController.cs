using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class FactoryLogController : Controller
    {
        private readonly ILogger _logger;

        public FactoryLogController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("test");
        }

        [HttpGet("test")]
        public IActionResult TestLog()
        {
            _logger.LogWarning("Warning Log");
            return Ok();
        }
    }
}