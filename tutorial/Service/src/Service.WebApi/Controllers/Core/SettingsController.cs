using System;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Entities;

namespace Service.WebApi.Controllers.Core
{
    [Route("api/core/settings"),
        GroupMeta(nameof(SettingsController))]
    public class SettingsController : Controller
    {
        private readonly CalculationSettings _calcSettings;
        
        public SettingsController(CalculationSettings calcSettings)
        {
            _calcSettings = calcSettings ?? throw new ArgumentNullException(nameof(calcSettings));
        }
        
        [HttpGet("read"), ActionMeta(nameof(ReadSettings))]
        public IActionResult ReadSettings()
        {
            return Ok(_calcSettings);
        }
    }
}