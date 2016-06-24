using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.Messages;

namespace RefArch.Infrastructure.Samples.RabbitMQ
{
    public class ExampleDirectExchange : DirectExchange<ExampleDirectEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleDirectExchange";

            QueueDeclare("2015-2016-Cars", config =>
            {
                config.RouteKeys = new[] { "2015", "2016" };
            });

            QueueDeclare("UsedCars", config =>
            {
                config.RouteKeys = new[] { "UsedModel" };
            });
        }
    }
}
