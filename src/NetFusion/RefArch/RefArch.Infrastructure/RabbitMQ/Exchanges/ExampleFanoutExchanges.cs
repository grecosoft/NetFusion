using NetFusion.Domain.Scripting;
using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.RabitMQ.Messages;

namespace RefArch.Infrastructure.RabbitMQ.Exchanges
{
    [ApplyScriptPredicate("HighImportanceCriteria", variableName: "IsHighImportance")]
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
            return message.Make == "Toyota" && message.Year > 2014;
        }
    }
}
