using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockWorkQueueExchange : WorkQueueExchange<MockDomainEvent>
    {
        public string FirstQueueName { get; set; }

        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "MockTestBrokerName";

            if (this.FirstQueueName != null)
                QueueDeclare(this.FirstQueueName, config =>
                {

                });
        }
    }
}
