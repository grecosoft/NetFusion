using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.WebApi.Controllers.Integration
{
    [ApiController, Route("api/integration/amqp")]
    [GroupMeta(nameof(AmqpController))]
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