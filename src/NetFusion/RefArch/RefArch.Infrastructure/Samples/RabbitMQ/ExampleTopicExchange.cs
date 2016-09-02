using NetFusion.Domain.Scripting;
using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.Messages.RabbitMQ;

namespace RefArch.Infrastructure.Samples.RabbitMQ
{
    [ApplyScriptPredicate("ClassicCarCriteria", PredicateAttributeName = "IsClassic")]
    public class ExampleTopicExchange : TopicExchange<ExampleTopicEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleTopicExchange";

            QueueDeclare("Chevy", config =>
            {
                config.RouteKeys = new[] { "Chevy.*.*" };
            });

            QueueDeclare("Chevy-Vette", config =>
            {
                config.RouteKeys = new[] { "Chevy.Vette.*" };
            });

            QueueDeclare("Ford", config =>
            {
                config.RouteKeys = new[] { "Ford.*.*", "Lincoln.*.*" };
            });
        }
    }
}
