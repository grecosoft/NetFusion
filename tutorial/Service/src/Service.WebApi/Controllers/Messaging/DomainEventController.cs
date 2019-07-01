using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Events;
using Service.Domain.Services;
using Service.WebApi.Models;

namespace Service.WebApi.Controllers.Messaging
{
    [ApiController, Route("api/messaging/domain-events")]
    [GroupMeta(nameof(CommandController))]
    public class DomainEventController : ControllerBase
    {
        private readonly IMessagingService _messaging;
        private readonly IExampleResultLog _exampleLog;
        
        public DomainEventController(
            IMessagingService messaging,
            IExampleResultLog exampleLog)
        {
            _messaging = messaging;
            _exampleLog = exampleLog;
        }

        [HttpPost("account"), ActionMeta(nameof(CreateAccount))]
        public async Task<IActionResult> CreateAccount([FromBody]AccountModel model)
        {
            var domainEvent = new NewAccountCreated(
                model.FirstName, 
                model.LastName, 
                model.AccountNumber);

            await _messaging.PublishAsync(domainEvent);

            return Ok(_exampleLog.GetLogs());
        }
    }
}