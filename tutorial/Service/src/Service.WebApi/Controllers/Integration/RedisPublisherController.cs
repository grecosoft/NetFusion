using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using Service.Domain.Events;

namespace Service.WebApi.Controllers.Integration
{
    [ApiController, Route("api/integration/redis")]
    public class RedisPublisherController : ControllerBase
    {
        private static IMessagingService _messaging;

        public RedisPublisherController(
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