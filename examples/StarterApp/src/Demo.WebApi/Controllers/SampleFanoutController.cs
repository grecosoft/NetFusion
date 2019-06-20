using System;
using System.Threading.Tasks;
using Demo.Domain.Events;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SampleFanoutController : Controller
    {
        private readonly IMessagingService _messaging;

        public SampleFanoutController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("temp/reading")]
        public Task TempReading([FromBody]TemperatureReading reading)
        {
            return _messaging.PublishAsync(reading);
        }
    }
}
