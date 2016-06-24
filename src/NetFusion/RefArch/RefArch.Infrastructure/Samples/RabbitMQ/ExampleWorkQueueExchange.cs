using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.Messages;

namespace RefArch.Infrastructure.Samples.RabbitMQ
{
    public class ExampleWorkQueueExchange : WorkQueueExchange<ExampleWorkQueueEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";

            QueueDeclare("ProcessSale", config =>
            {

            });

            QueueDeclare("ProcessService", config =>
            {

            });
        }
    }
}