using System;
using Microsoft.AspNetCore.Mvc;
using Service.Domain.Entities;

namespace Service.WebApi.Controllers.Core
{
    [ApiController, Route("api/core/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly CalculationSettings _calcSettings;
        
        public SettingsController(CalculationSettings calcSettings)
        {
            _calcSettings = calcSettings ?? throw new ArgumentNullException(nameof(calcSettings));
        }
        
        [HttpGet("read")]
        public IActionResult ReadSettings()
        {
            return Ok(_calcSettings);
        }
    }
}