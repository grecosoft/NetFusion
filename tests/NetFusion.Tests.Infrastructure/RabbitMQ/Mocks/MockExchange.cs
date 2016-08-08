using NetFusion.RabbitMQ.Exchanges;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockExchange : DirectExchange<MockDomainEvent>
    {

        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "MockTestBrokerName";
            Settings.ExchangeName = "MockDirectExchangeName";

            QueueDeclare("MockTestQueueName", config =>
            {
                config.RouteKeys = new[] { "RouteKeyOne" };
            });
            
        }
    }
}
