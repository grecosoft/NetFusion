using NetFusion.RabbitMQ.Core;
using RefArch.Api.Messages.RabbitMQ;

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