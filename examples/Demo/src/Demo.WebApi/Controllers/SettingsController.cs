using Demo.App;
using Microsoft.AspNetCore.Mvc;

namespace Demo.WebApi.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ServiceSettings _settings;

        public SettingsController(ServiceSettings settings)
        {
            _settings = settings;
        }

        [HttpGet()]
        public IActionResult GetSettings()
        {
            return Ok(_settings);
        }
    }
}
