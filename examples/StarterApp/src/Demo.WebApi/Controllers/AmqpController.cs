using System;
using System.Threading.Tasks;
using Demo.Domain.Commands;
using Demo.Domain.Events;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/integration/amqp")]
    [ApiController]
    public class AmqpController : ControllerBase
    {
        private readonly IMessagingService _messaging;

        public AmqpController(IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("submission")]
        public async Task<IActionResult> SendClaimSubmission([FromBody]CreateClaimSubmission submission)
        {
            await _messaging.SendAsync(submission);
            return Ok();
        }
        
        [HttpPost("status-updated")]
        public async Task<IActionResult> PublishClaimStatus([FromBody] ClaimStatusUpdated status)
        {
            await _messaging.PublishAsync(status);
            return Ok();
        }
    }
}
