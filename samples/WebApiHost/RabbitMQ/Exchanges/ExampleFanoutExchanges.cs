using ExampleApi.Messages;
using NetFusion.Base.Scripting;
using NetFusion.RabbitMQ.Exchanges;

namespace WebApiHost.RabbitMQ.Exchanges
{
    [ApplyScriptPredicate("HighImportanceCriteria", attributeName: "IsHighImportance")]
    public class HighImportanceExchange : FanoutExchange<ExampleFanoutEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "HighImportanceExchange";
        }
    }

    public class LowImportanceExchange : FanoutExchange<ExampleFanoutEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "LowImportanceExchange";
        }

        protected override bool Matches(ExampleFanoutEvent message)
        {
            return message.Make.Equals("Toyota", System.StringComparison.OrdinalIgnoreCase) && message.Year > 2014;
        }
    }
}
