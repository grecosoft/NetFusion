using System.Collections.Generic;
using EasyNetQ;
using EasyNetQ.Topology;
using Moq;
using NetFusion.RabbitMQ.Plugin.Modules;
using NetFusion.RabbitMQ.Subscriber.Internal;

namespace IntegrationTests.RabbitMQ
{
    using NetFusion.RabbitMQ.Metadata;

    public class MockSubscriberModule : SubscriberModule
    {
        public List<QueueMeta> CreatedQueues { get; } = new List<QueueMeta>();
        public List<MessageQueueSubscriber> Subscribers { get; } = new List<MessageQueueSubscriber>();

        protected override IQueue QueueDeclare(IBus bus, QueueMeta queueMeta)
        {
            CreatedQueues.Add(queueMeta);
            
            var mockQueue = new Mock<IQueue>();
            return mockQueue.Object;
        }
        
        protected override void ConsumeMessageQueue(IBus bus, IQueue queue, MessageQueueSubscriber subscriber)
        {
            Subscribers.Add(subscriber);
        }
    }
}