using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NetFusion.AMQP.Subscriber
{
    /// <summary>
    /// Hosted service when registered within an application host's service
    /// collection will subscribe to Amqp host items such as queues and topics
    /// and invokes the corresponding method handler.
    /// </summary>
    public class AmqpSubscriberHostedService : IHostedService
    {
        private readonly ISubscriberModule _subscriberModule;
        private readonly ISubscriptionSettings _subscriptionSettings;
        
        public AmqpSubscriberHostedService(ISubscriberModule subscriberModule, 
            ISubscriptionSettings subscriptionSettings)
        {
            _subscriberModule = subscriberModule ?? throw new ArgumentNullException(nameof(subscriberModule));
            _subscriptionSettings = subscriptionSettings ?? throw new ArgumentNullException(nameof(subscriptionSettings));
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriptionSettings.ConfigureSettings();
            await _subscriberModule.LinkHandlersToHostItems(_subscriptionSettings);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _subscriptionSettings.CleanupSettings();
        }
    }
}