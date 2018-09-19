namespace Demo.WebApi
{
    using System;
    using System.Threading.Tasks;
    using NetFusion.Common.Extensions;
    using NetFusion.Redis.Subscriber;

    public class TestSubscriptionService : ITestSubscriptionService
    {
        private readonly ISubscriptionService _subscription;
        
        public TestSubscriptionService(ISubscriptionService subscription)
        {
            _subscription = subscription;
        }

        public Task AddSubscription(string channel)
        {
            return _subscription.SubscribeAsync("testdb", channel,
                (AutoPurchasedEvent evt) =>
                {
                   Console.WriteLine(evt.ToIndentedJson()); 
                });
        }

        public Task RemoveSubscription(string channel)
        {
            return _subscription.UnSubscribeAsync("testdb", channel);
        }
    }
}