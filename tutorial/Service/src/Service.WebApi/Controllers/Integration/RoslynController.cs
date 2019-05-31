using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Base.Scripting;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Entities;

namespace Service.WebApi.Controllers.Integration
{
    [Route("api/integration/roslyn/expressions"),
     GroupMeta(nameof(RoslynController))]
    public class RoslynController : Controller
    {
        private readonly IEntityScriptingService _scripting;
        
        public RoslynController(IEntityScriptingService scriptingService)
        {
            _scripting = scriptingService;
        }

        [HttpPatch("sensor/evaluate"), ActionMeta(nameof(EvaluateSensor))]
        public async Task<IActionResult> EvaluateSensor([FromBody]Sensor sensor)
        {
            sensor.SensorId = Guid.NewGuid();
            await _scripting.ExecuteAsync(sensor);

            return Ok(sensor);
        }
    }
}