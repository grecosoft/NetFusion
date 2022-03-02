using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Strategies;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Contains information specific to queue subscriptions.
    /// </summary>
    public class QueueSubscription : EntitySubscription
    {
        public QueueSubscription(string namespaceName, string queueName)
            : base(namespaceName, queueName)
        {
            SubscriptionStrategy = new QueueSubscriptionStrategy(this);
        }

        public override string ToString() => $"{NamespaceName}:{EntityName}->{DispatchInfo}";
    }
}