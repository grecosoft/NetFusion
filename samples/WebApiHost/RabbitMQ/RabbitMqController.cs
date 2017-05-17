using ExampleApi.Messages;
using ExampleApi.Models;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using System.Threading.Tasks;
using NetFusion.Web.Mvc.Metadata;

namespace WebApiHost.RabbitMQ
{
    [Route("api/[controller]")]
    [GroupMeta]
    public class RabbitMqController : Controller
    {
        private IMessagingService _messagingSrv;

        public RabbitMqController(
            IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
        }

        [HttpPost("direct", Name = "PublishDirectEvent"), ActionMeta("PublishDirectEvent")]
        public Task PublishDirectEvent([FromBody]Car car)
        {
            var evt = new ExampleDirectEvent(car);
            return _messagingSrv.PublishAsync(evt);
        }

        [HttpPost("topic", Name = "PublishTopicEvent"), ActionMeta("PublishTopicEvent")]
        public Task PublishTopicEvent([FromBody]Car car)
        {
            var evt = new ExampleTopicEvent(car);
            //evt.SetContentType(SerializerTypes.Binary);
            return _messagingSrv.PublishAsync(evt);
        }

        [HttpPost("fanout", Name = "PublishFanoutEvent"), ActionMeta("PublishFanoutEvent")]
        public Task PublishFanoutEvent([FromBody]Car car)
        {
            var evt = new ExampleFanoutEvent(car);
            return _messagingSrv.PublishAsync(evt);
        }

        [HttpPost("work-queue", Name = "PublishWorkQueueEvent"), ActionMeta("PublishWorkQueueEvent")]
        public Task PublishWorkQueueEvent([FromBody]Car car)
        {
            var evt = new ExampleWorkQueueEvent(car);
            return _messagingSrv.PublishAsync(evt);
        }

        [HttpPost("rpc", Name = "PublishRPCEvent"), ActionMeta("PublishRpcEvent")]
        public async Task<string> PublishRPCEvent([FromBody]Car car)
        {
            var evt = new ExampleRpcCommand(car);
            var response = await _messagingSrv.PublishAsync(evt);
            return response.ResponseTestValue;
        }
    }
}
