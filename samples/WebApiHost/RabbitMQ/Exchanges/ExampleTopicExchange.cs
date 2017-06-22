using ExampleApi.Messages;
using NetFusion.RabbitMQ.Exchanges;

namespace WebApiHost.RabbitMQ.Exchanges
{
    public class ExampleTopicExchange : TopicExchange<ExampleTopicEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleTopicExchange";

            QueueDeclare("VW-GTI", config =>
            {
                config.RouteKeys = new[] { "VW.GTI.*.*" };
            });

            QueueDeclare("VW-BLACK", config =>
            {
                config.RouteKeys = new[] { "VW.*.*.BLACK" };
            });

            QueueDeclare("AUDI");
        }
    }
}
