using NetFusion.Messaging;
using NetFusion.WebApi.Metadata;
using RefArch.Api.Messages.RabbitMQ;
using RefArch.Api.Models;
using System.Threading.Tasks;
using System.Web.Http;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Serialization;
using System.Collections.Generic;

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
            evt.Attributes.Values.Test = "AAAABBBBB";
            await _messagingSrv.PublishAsync(evt);
        }

        [HttpPost, Route("topic", Name = "PublishTopicEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishTopicEvent(Car car)
        {
            var evt = new ExampleTopicEvent(car);
            evt.SetContentType(SerializerTypes.Binary);
            await _messagingSrv.PublishAsync(evt);
        }
 
        [HttpPost, Route("fanout", Name = "PublishFanoutEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishFanoutEvent(Car car)
        {
            var evt = new ExampleFanoutEvent(car);
            await _messagingSrv.PublishAsync(evt);
        }
    
        [HttpPost, Route("work-queue", Name = "PublishWorkQueueEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task PublishWorkQueueEvent(Car car)
        {
            var evt = new ExampleWorkQueueEvent(car);
            await _messagingSrv.PublishAsync(evt);
        }

        [HttpPost, Route("rpc", Name = "PublishRPCEvent")]
        [RouteMetadata(IncludeRoute = true)]
        public async Task<string> PublishRPCEvent(Car car)
        {
            var evt = new ExampleRpcCommand(car);
            var response = await _messagingSrv.PublishAsync(evt);
            return response.ResponseTestValue;
        }
    }
}

