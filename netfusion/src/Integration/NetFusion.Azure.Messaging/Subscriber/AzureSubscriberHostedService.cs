using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NetFusion.Azure.Messaging.Subscriber
{
    /// <summary>
    /// Hosted service when registered within an application host's service
    /// collection will subscribe to Azure Service Bus namespace items such
    /// as queues and topic and invokes the corresponding method handler.
    /// </summary>
    public class AzureSubscriberHostedService : IHostedService
    {
        private readonly ISubscriberModule _subscriberModule;
        private readonly ISubscriptionSettings _subscriptionSettings;
        
        public AzureSubscriberHostedService(ISubscriberModule subscriberModule, 
            ISubscriptionSettings subscriptionSettings)
        {
            _subscriberModule = subscriberModule ?? throw new ArgumentNullException(nameof(subscriberModule));
            _subscriptionSettings = subscriptionSettings ?? throw new ArgumentNullException(nameof(subscriptionSettings));
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _subscriptionSettings.ConfigureSettings();
            await _subscriberModule.LinkHandlersToNamespaces(_subscriptionSettings);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}