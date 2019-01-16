namespace Service.WebApi.Controllers.Integration
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NetFusion.Messaging;
    using NetFusion.Web.Mvc.Metadata;
    using Service.Domain.Events;

    [Route("api/integration/redis"),
     GroupMeta(nameof(MongoDbController))]
    public class RedisPublisherController : Controller
    {
        private static IMessagingService _messaging;

        public RedisPublisherController(
            IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("order/submitted"), ActionMeta(nameof(SubmitOrder))]
        public Task SubmitOrder([FromBody] OrderSubmitted order)
        {
            return _messaging.PublishAsync(order);
        }
    }
}