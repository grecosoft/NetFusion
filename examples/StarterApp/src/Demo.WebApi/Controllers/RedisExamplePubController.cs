using System.Threading.Tasks;
using Demo.Domain.Events;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/publisherexample")]
    public class RedisExamplePubController : Controller
    {
        private static IMessagingService _messaging;

        public RedisExamplePubController(
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
