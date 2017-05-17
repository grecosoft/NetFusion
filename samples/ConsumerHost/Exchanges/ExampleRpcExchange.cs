using NetFusion.RabbitMQ.Exchanges;

namespace ConsumerHost.Exchanges
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
