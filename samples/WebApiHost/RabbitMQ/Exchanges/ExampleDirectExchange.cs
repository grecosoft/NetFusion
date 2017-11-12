using ExampleApi.Messages;
using NetFusion.Base.Scripting;
using NetFusion.RabbitMQ.Exchanges;

namespace WebApiHost.RabbitMQ.Exchanges
{
    [ApplyScriptPredicate("ClassicCarCriteria", attributeName: "IsClassic")]
    public class ExampleDirectExchange : DirectExchange<ExampleDirectEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleDirectExchange";

            QueueDeclare("GENERAL-MOTORS", config =>
            {
                config.RouteKeys = new[] { "CHEVY", "Buick", "GMC", "CADILLAC" };
            });

            QueueDeclare("VOLKSWAGEN", config =>
            {
                config.RouteKeys = new[] { "VW", "Audi", "PORSCHE", "BENTLEY", "LAMBORGHINI" };
            });
        }
    }
}
