using System;
using System.Threading.Tasks;
using Demo.Domain.Commands;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleWorkQueueController : ControllerBase
    {
        private readonly IMessagingService _messaging;

        public SampleWorkQueueController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("send/email")]
        public Task SendEmail([FromBody]SendEmail command)
        {
            return _messaging.SendAsync(command);
        }
    }
}
