namespace Demo.WebApi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using NetFusion.Common.Extensions;
    using NetFusion.Redis.Subscriber;

    public class TestHostedService : IHostedService
    {
        private readonly ISubscriptionService _subscriptions;
        
        public TestHostedService(
            ISubscriptionService subscriptions)
        {
            _subscriptions = subscriptions;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriptions.SubscribeAsync("testdb", "auto-purchases.VW.*.2017", 
                (AutoPurchasedEvent evt) =>
                {
                    Console.WriteLine(evt.ToIndentedJson());
                }, cancellationToken: cancellationToken);
            
            await _subscriptions.SubscribeAsync("testdb", "auto-purchases.*.*.2017", 
                (AutoPurchasedEvent evt) =>
                {
                    Console.WriteLine(evt.ToIndentedJson());
                },
                cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _subscriptions.UnSubscribeAsync("testdb", "auto-purchases.VW.*.2017", cancellationToken),
                _subscriptions.UnSubscribeAsync("testdb", "auto-purchases.*.*.2017", cancellationToken));
        }
    }
}