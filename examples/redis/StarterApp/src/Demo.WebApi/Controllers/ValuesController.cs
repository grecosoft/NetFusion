using Microsoft.AspNetCore.Mvc;

namespace Demo.WebApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using NetFusion.Messaging;
    using NetFusion.Redis.Modules;

    [Route("api/values")]
    public class ValuesController: Controller
    {
        private readonly IMessagingService _messaging;
        private readonly ITestSubscriptionService _subService;
        
        public ValuesController(
            IMessagingService messaging,
            ITestSubscriptionService subService)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
            _subService = subService ?? throw new ArgumentNullException(nameof(subService));
        }

        [HttpPost]
        public Task PublishDomainEvent([FromBody] AutoPurchasedEvent evt)
        {
            return _messaging.PublishAsync(evt);
        }

        [HttpPost("subscribe")]
        public Task AddSubscription([FromBody] string channel)
        {
            return _subService.AddSubscription(channel);
        }

        [HttpPost("unsubscribe")]
        public Task RemoveSubscription([FromBody] string channel)
        {
            return _subService.RemoveSubscription(channel);
        }
    }
}
