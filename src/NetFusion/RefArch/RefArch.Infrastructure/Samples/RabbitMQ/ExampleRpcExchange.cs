using NetFusion.RabbitMQ.Exchanges;
using RefArch.Api.Messages;

namespace RefArch.Infrastructure.Samples.RabbitMQ
{
    public class ExampleRpcExchange : DirectExchange<ExampleRpcCommand>
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";
            Settings.ExchangeName = "SampleRpcExchange";
            SetReturnType<ExampleRpcResponse>();

            QueueDeclare("QueueWithConsumerResponse", config =>
            {
                config.RouteKeys = new[] { "Hello" };
            });
        }
    }
}
