using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    using Domain.Events;

    [Route("api/[controller]")]
    public class PublisherExampleController : Controller
    {
        private static IMessagingService _messaging;

        public PublisherExampleController(
            IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("order/submitted")]
        public Task SubmitOrder([FromBody] OrderSubmitted order)
        {
            return _messaging.PublishAsync(order);
        }
    }
}