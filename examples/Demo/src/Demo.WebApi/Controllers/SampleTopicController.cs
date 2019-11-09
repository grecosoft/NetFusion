using System;
using System.Threading.Tasks;
using Demo.Domain.Events;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleTopicController : ControllerBase
    {
        private readonly IMessagingService _messaging;

        public SampleTopicController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("auto/sales")]
        public Task RecordAutoSale([FromBody]AutoSaleCompleted salesCompleted)
        {
            salesCompleted.SetRouteKey(
                salesCompleted.Make,
                salesCompleted.Model,
                salesCompleted.Year);

            return _messaging.PublishAsync(salesCompleted);
        }
    }
}
