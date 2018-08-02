using System;
using System.Threading.Tasks;
using Demo.WebApi.Commands;
using Demo.WebApi.DomainEvents;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class PublisherController : Controller
    {
        private readonly IMessagingService _messaging;

        public PublisherController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("auto/sales")]
        public Task RecordAutoSale([FromBody]AutoModel model)
        {
            var salesCompleted = new AutoSaleCompleted(
                model.Make,
                model.Model,
                model.Year,
                model.Color)
            {
                IsNew = model.IsNew
            };

            return _messaging.PublishAsync(salesCompleted);
        }
        
        
        [HttpPost("domain-event")]
        public async Task DomainEvent()
        {
            var e = new TestDomainEvent("AUDI", "A4");
            await _messaging.PublishAsync(e);
        }

        [HttpPost("command")]
        public async Task Command()
        {
            var c = new TestCommand();
            await _messaging.SendAsync(c);
        }

        [HttpPost("domain-notification")]
        public async Task DomainNotification()
        {
            var e = new NotificationDomainEvent("Ladder");
            await _messaging.PublishAsync(e);
        }

        [HttpPost("rpc-command")]
        public async Task RpcCommand()
        {
            var c = new GetPropertyTaxCommand();
            var result = await _messaging.SendAsync(c);

            Console.WriteLine(result.Value);
        }
    }
}
