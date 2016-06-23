using NetFusion.Messaging;
using NetFusion.WebApi.Metadata;
using RefArch.Api.Events;
using RefArch.Api.Models;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.RabbitMQ", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/rabbitmq")]
    public class RabbitMQController : ApiController
    {
        private IMessagingService _messagingSrv;

        public RabbitMQController(
            IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
        }

        [HttpPost, Route("direct", Name = "PublishDirectEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishDirectEvent(Car car)
        {
            var evt = new ExampleDirectEvent(car);
            await _messagingSrv.PublishAsync(evt);
        }
    }
}