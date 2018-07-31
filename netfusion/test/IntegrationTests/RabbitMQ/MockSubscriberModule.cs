using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using EasyNetQ;
using EasyNetQ.Topology;
using Moq;
using NetFusion.RabbitMQ.Modules;
using NetFusion.RabbitMQ.Subscriber.Internal;

namespace IntegrationTests.RabbitMQ
{
    public class MockSubscriberModule : SubscriberModule
    {
        public List<QueueExchangeDefinition> CreatedExchanges { get; } = new EditableList<QueueExchangeDefinition>();
        public List<QueueContext> CreatedQueues { get; } = new EditableList<QueueContext>();
        public List<MessageQueueSubscriber> Subscribers = new EditableList<MessageQueueSubscriber>();

        protected override IExchange CreateExchange(IBus bus, QueueExchangeDefinition definition)
        {
            CreatedExchanges.Add(definition);
            
            var mockExchange = new Mock<IExchange>();
            return mockExchange.Object;
        }

        protected override IQueue CreateQueue(MessageQueueSubscriber subscriber, QueueContext queueContext)
        {
            CreatedQueues.Add(queueContext);
            
            var mockQueue = new Mock<IQueue>();
            return mockQueue.Object;
        }

        protected override void ConsumeMessageQueue(IBus bus, IQueue queue, MessageQueueSubscriber subscriber)
        {
            Subscribers.Add(subscriber);
        }
    }
}