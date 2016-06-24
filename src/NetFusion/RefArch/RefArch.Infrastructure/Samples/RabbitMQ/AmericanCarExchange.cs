using NetFusion.Common.Extensions;
using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.Messages;

namespace RefArch.Infrastructure.Samples.RabbitMQ
{
    public class AmericanCarExchange : FanoutExchange<ExampleFanoutEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleFanoutExchange->AmericanCars";
        }

        // This is optional and is called to determine if the exchange should be
        // sent the message.
        protected override bool Matches(ExampleFanoutEvent message)
        {
            return message.Make.InSet("Ford", "GMC", "Chevy");
        }
    }
}
