using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockDirectExchange : DirectExchange<MockDomainEvent>
    {
        public string FirstQueueName { get; set; } 
        public string SecondQueueName { get; set; }
        public string[] FirstQueueRouteKeys { get; set; } = new string[]{"High", "Important"};
        public string[] SecondQueueRouteKeys { get; set; } = new string[] { "Low", "Not-Important" };

        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "MockTestBrokerName";
            Settings.ExchangeName = "MockDirectExchangeName";

            if (this.FirstQueueName != null)
            {
                QueueDeclare(this.FirstQueueName, config =>
                {
                    config.RouteKeys = this.FirstQueueRouteKeys;
                });
            }

            if (this.SecondQueueName != null)
            {
                QueueDeclare(this.SecondQueueName, config =>
                {
                    config.RouteKeys = this.SecondQueueRouteKeys;
                });
            }
        }
    }
}
