using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.RabitMQ.Messages;

namespace RefArch.Infrastructure.RabbitMQ.Exchanges
{
    public class ExampleWorkQueueExchange : WorkQueueExchange<ExampleWorkQueueEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";

            QueueDeclare("ProcessSale");
            QueueDeclare("ProcessService");
        }
    }
}