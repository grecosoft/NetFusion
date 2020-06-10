using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Base.Scripting;
using Service.Domain.Entities;

namespace Service.WebApi.Controllers.Integration
{
    [ApiController, Route("api/integration/roslyn/expressions")]
    public class RoslynController : ControllerBase
    {
        private readonly IEntityScriptingService _scripting;
        
        public RoslynController(IEntityScriptingService scriptingService)
        {
            _scripting = scriptingService;
        }

        [HttpPatch("sensor/evaluate")]
        public async Task<IActionResult> EvaluateSensor([FromBody]Sensor sensor)
        {
            sensor.SensorId = Guid.NewGuid();
            await _scripting.ExecuteAsync(sensor);

            return Ok(sensor);
        }
    }
}