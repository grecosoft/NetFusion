using NetFusion.Common.Extensions;
using NetFusion.RabbitMQ.Core;
using RefArch.Api.Messages.RabbitMQ;

namespace RefArch.Infrastructure.Samples.RabbitMQ
{
    public class GermanCarExchange : FanoutExchange<ExampleFanoutEvent>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleFanoutExchange->GermanCars";
        }

        // This is optional and is called to determine if the exchange should be
        // sent the message.
        protected override bool Matches(ExampleFanoutEvent message)
        {
            return message.Make.InSet("VW", "Audi", "BMW");
        }
    }
}
