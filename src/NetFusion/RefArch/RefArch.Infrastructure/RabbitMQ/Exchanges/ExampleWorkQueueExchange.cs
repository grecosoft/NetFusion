using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.RabitMQ.Messages;

namespace RefArch.Infrastructure.RabbitMQ.Exchanges
{
    public class ExampleWorkQueueExchange : WorkQueueExchange<ExampleWorkQueueEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";

            QueueDeclare("process_sale");
            QueueDeclare("PROCESS_SERVICE");
        }
    }
}