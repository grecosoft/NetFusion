using NetFusion.RabbitMQ.Core;

namespace NetFusion.RabbitMQ.Configs
{
    public class RpcConsumerSettings 
    {
        public string RequestQueueKey { get; set; }
        public string RequestQueueName { get; set; }
        public string ContentType { get; set; } = ExchangeSettings.DefaultContentType;
    }
}
