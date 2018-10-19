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
        
        public AzureSubscriberHostedService(ISubscriberModule subscriberModule)
        {
            _subscriberModule = subscriberModule ?? throw new ArgumentNullException(nameof(subscriberModule));
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _subscriberModule.LinkHandlersToNamespaces();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}