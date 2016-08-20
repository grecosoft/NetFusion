using NetFusion.RabbitMQ.Core;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockExchange : DirectExchange<MockDomainEvent>
    {
        public bool IsDurable { get; set; }
        public bool IsAutoDelete { get; set; }
 
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "MockTestBrokerName";
            Settings.ExchangeName = "MockDirectExchangeName";
            Settings.IsDurable = this.IsDurable;
            Settings.IsAutoDelete = this.IsAutoDelete;

            QueueDeclare("MockTestQueueName", config =>
            {
                config.RouteKeys = new[] { "RouteKeyOne" };
            });
            
        }
    }
}
