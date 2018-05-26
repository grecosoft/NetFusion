using System.Threading.Tasks;
using Demo.App;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/messaging/extensions")]
    public class ExtensionsController : Controller 
    {
        private readonly IMessagingService _messaging;

        public ExtensionsController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpGet ("new-customer")]
        public Task TestPublisher()
        {
            var registration = new RegistrationDomainEvent("Lisa", "Smith");
            return _messaging.PublishAsync(registration);
        }
    }
}