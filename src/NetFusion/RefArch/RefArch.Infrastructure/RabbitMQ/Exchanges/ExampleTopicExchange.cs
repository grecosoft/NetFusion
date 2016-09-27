using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.RabitMQ.Messages;

namespace RefArch.Infrastructure.RabbitMQ.Exchanges
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

            QueueDeclare("AUDI", config =>
            {
                config.RouteKeys = new[] { "Audi.*.*.*" };
            });
        }
    }
}
