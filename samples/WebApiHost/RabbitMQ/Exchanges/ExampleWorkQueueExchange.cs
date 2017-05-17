using ExampleApi.Messages;
using NetFusion.RabbitMQ.Exchanges;

namespace WebApiHost.RabbitMQ.Exchanges
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