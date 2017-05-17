using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    [Broker("MockTestBrokerName")]
    public class MockMessageConsumer : IMessageConsumer
    {
        [JoinQueue("MockTestQueueName", "MockDirectExchangeName")]
        public void OnJoin(MockDomainEvent evt)
        {

        }

        [AddQueue("MockDirectExchangeName", RouteKey = "#",
            IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
        public void OnAdd(MockDomainEvent evt)
        {

        }

    }
}
