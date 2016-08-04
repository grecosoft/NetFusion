using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockTopicExchange : TopicExchange<MockDomainEvent>
    {
        public string FirstQueueName { get; set; }
        public string[] FirstQueueRouteKeys { get; set; } = new string[] { "Status.High.*", "High Status" };

        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "MockTestBrokerName";
            Settings.ExchangeName = "MockTopicExchangeName";

            if (this.FirstQueueName != null)
            {
                QueueDeclare(this.FirstQueueName, config =>
                {
                    config.RouteKeys = this.FirstQueueRouteKeys;
                });
            }
        }
    }
}
