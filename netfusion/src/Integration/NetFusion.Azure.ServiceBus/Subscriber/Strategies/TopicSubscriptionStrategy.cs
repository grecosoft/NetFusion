using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Subscriber.Strategies
{
    /// <summary>
    /// Strategy subscribing and processing Domain Event messages received on a topic.
    /// </summary>
    public class TopicSubscriptionStrategy : ISubscriptionStrategy,
        IRequiresContext
    {
        public NamespaceContext Context { get; set; }
        private readonly TopicSubscription _subscription;
        
        public TopicSubscriptionStrategy(TopicSubscription subscription)
        {
            _subscription = subscription;
        }

        public async Task Subscribe(NamespaceConnection connection)
        {
            SetSubscriptionTypeProperties();
            
            await CreateTempTopic(connection);
            await connection.CreateOrUpdateSubscription(_subscription);
            await SubscribeToSubscription(connection);
        }

        // In the case of a Fanout subscription used for notification type message delivery,
        // received domain-events when the Microservice instance is not running should be
        // ignored so the subscription will auto-delete after being un-active for 5 minutes
        // if not deleted when the Microservice is stopped.
        private void SetSubscriptionTypeProperties()
        {
            if (_subscription.IsFanout)
            {
                _subscription.AutoDeleteOnIdle = TimeSpan.FromMinutes(5);
            }
        }

        // In a Microservice architecture, it is not guaranteed that the publishing service,
        // responsible for creating topics, will be started before consumers of the topics.
        // This creates a temporary topic that is subscribed and reconfigured when the
        // publisher starts.
        private async Task CreateTempTopic(NamespaceConnection connection)
        {
            if (! await connection.AdminClient.TopicExistsAsync(_subscription.EntityName))
            {
                try
                {
                    await connection.AdminClient.CreateTopicAsync(_subscription.EntityName);
                }
                catch (ServiceBusException ex) 
                    when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    // Ignore exception if the topic was created between checking
                    // if existed and issuing creation request.
                }
            }
        }
        
        private Task SubscribeToSubscription(NamespaceConnection connection)
        {
            ServiceBusProcessor processor = connection.BusClient.CreateProcessor(
                _subscription.EntityName, _subscription.UniqueSubscriptionName, _subscription.Options);
            
            _subscription.ProcessedBy(processor);

            processor.ProcessMessageAsync += OnMessageReceived;
            processor.ProcessErrorAsync += OnProcessingError;
            
            return processor.StartProcessingAsync();
        }
        
        private async Task OnMessageReceived(ProcessMessageEventArgs args)
        {
            IMessage message = Context.DeserializeMessage(_subscription.DispatchInfo, args);
            
            Context.LogSubscription(_subscription);
            
            // Delegate to the In-Process message handler associated with the Domain Event.
            await Context.DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
                _subscription.DispatchInfo, 
                message);
        }

        private Task OnProcessingError(ProcessErrorEventArgs args)
        {
            Context.LogProcessError(args);
            return Task.CompletedTask;
        }
    }
}