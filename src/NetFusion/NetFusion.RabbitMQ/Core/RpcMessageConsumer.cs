using NetFusion.RabbitMQ.Configs;

namespace NetFusion.RabbitMQ.Core
{
    public class RpcMessageConsumer
    {
        public IRpcClient Client { get; }
        public string BrokerName { get; }
        public string RequestQueueKey { get; }
        public string ContentType { get; }

        public RpcMessageConsumer(
            string brokerName, 
            RpcConsumerSettings settings, 
            IRpcClient client)
        {
            this.BrokerName = brokerName;
            this.RequestQueueKey = settings.RequestQueueKey;
            this.ContentType = settings.ContentType;
            this.Client = client;
        }
    }
}
