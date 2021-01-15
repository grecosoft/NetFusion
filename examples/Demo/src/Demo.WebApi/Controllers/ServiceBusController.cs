using System.Threading.Tasks;
using Demo.Domain.Commands;
using Demo.Domain.Events;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceBusController : ControllerBase
    {
        private readonly IMessagingService _messaging;
        
        public ServiceBusController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("command/no-response")]
        public async Task<IActionResult> CommandWithNoResponse(IssueMembershipCard command)
        {
            await _messaging.SendAsync(command);
            return Ok("Command Sent");
        }
        
        [HttpPost("command/with-response")]
        public async Task<IActionResult> CommandWithResponse(UpdateCarFaxReport command)
        {
            await _messaging.SendAsync(command);
            return Ok("Command Sent");
        }
        
        [HttpPost("command/rpc-response")]
        public async Task<IActionResult> RpcCommandResponse(CalculateRange command)
        {
            var response = await _messaging.SendAsync(command);
            return Ok(response);
        }
        
        [HttpPost("domain-event/topic")]
        public async Task<IActionResult> DomainEventWithTopic(NewListing domainEvent)
        {
            await _messaging.PublishAsync(domainEvent);
            return Ok("Domain-Event Published");
        }
        
        [HttpPost("domain-event/topic/fan-out")]
        public async Task<IActionResult> DomainEventWithFanOutTopic(NewSubmission domainEvent)
        {
            await _messaging.PublishAsync(domainEvent);
            return Ok("Domain-Event Published");
        }
    }
}