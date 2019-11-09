using System.Threading.Tasks;
using Demo.Domain.Queries;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueryFilterController : ControllerBase
    {
        private readonly IMessagingService _messaging;
        
        public QueryFilterController(
            IMessagingService messaging)
        {
            _messaging = messaging;
        }
        
        [HttpGet("devices/{deviceId}/data")]
        public async Task<IActionResult> ReadSensorData([FromQuery]string deviceId)
        {
            var query = new QuerySensorData(deviceId);
            var reading = await _messaging.DispatchAsync(query);

            return Ok(reading);
        }
    }
}