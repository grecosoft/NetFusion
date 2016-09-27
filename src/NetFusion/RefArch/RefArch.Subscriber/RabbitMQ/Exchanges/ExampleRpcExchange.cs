using NetFusion.RabbitMQ.Exchanges;

namespace RefArch.Subscriber.RabbitMQ.Exchanges
{
    public class ExampleRpcExchange : RpcExchange
    {
        protected override void OnDeclareExchange()
        {
            Settings.BrokerName = "TestBroker";

            QueueDeclare("RpcMessageQueue");
        }
    }
}
