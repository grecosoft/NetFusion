using NetFusion.Domain.Scripting;
using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.RabitMQ.Messages;

namespace RefArch.Infrastructure.RabbitMQ.Exchanges
{
    [ApplyScriptPredicate("ClassicCarCriteria", variableName: "IsClassic")]
    public class ExampleDirectExchange : DirectExchange<ExampleDirectEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleDirectExchange";

            QueueDeclare("GENERAL-MOTORS", config =>
            {
                config.RouteKeys = new[] { "CHEVY", "BUICK", "GMC", "CADILLAC" };
            });

            QueueDeclare("VOLKSWAGEN", config =>
            {
                config.RouteKeys = new[] { "VW", "AUDI", "PORSCHE", "BENTLEY", "LAMBORGHINI" };
            });
        }
    }
}
