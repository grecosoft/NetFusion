using System;
using System.Threading.Tasks;
using Demo.Domain.Events;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SampleDirectController : Controller
    {
        private readonly IMessagingService _messaging;

        public SampleDirectController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("property/sales")]
        public Task RecordPropertySale([FromBody]PropertySold propertyEvent)
        {
            propertyEvent.SetRouteKey(propertyEvent.State);
            return _messaging.PublishAsync(propertyEvent);
        }
    }
}
