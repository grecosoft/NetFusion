using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.WebApi.Controllers.Integration
{
    [ApiController, Route("api/integration/service-bus")]
    public class ServiceBusController : ControllerBase
    {
        private readonly IMessagingService _messaging;
        
        public ServiceBusController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("topic-1")]
        public async Task<IActionResult> PublishDomainEventToTopic(HealthAlertDetected alert)
        {
            // alert.SetTimeToLive(TimeSpan.FromMinutes(1));
            await _messaging.PublishAsync(alert);
            return Ok();
        }

        [HttpPost("queue-1")]
        public async Task<IActionResult> GenerateEmail(SendEmail command)
        {
            await _messaging.SendAsync(command);
            return Ok();
        }

        [HttpPost("queue-2")]
        public async Task<IActionResult> GenerateData(GenerateData command)
        {
            await _messaging.SendAsync(command);
            return Ok();
        }
        
        [HttpPost("queue-3")]
        public async Task<IActionResult> SendRpcCommand(CalculateTradeInValue command)
        {
            var result = await _messaging.SendAsync(command);
            return Ok(result);
        }
    }
}