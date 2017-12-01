using NetFusion.RabbitMQ.Configs;
using System;

namespace NetFusion.RabbitMQ.Core.Rpc
{
    /// <summary>
    /// Aggregates a configured RPC broker client and the queue that should be
    /// used to make RPC style requests.  Also contains an instance of the client
    /// that makes the actual request to the consumer. 
    /// </summary>
    public class RpcMessagePublisher
    {
        public string BrokerName { get; }
        public IRpcClient Client { get; }

        public string RequestQueueKey { get; }
        public string RequestQueueName { get; }
        public string ContentType { get; }

        public RpcMessagePublisher(
            string brokerName, 
            RpcConsumerSettings settings, 
            IRpcClient client)
        {
            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Broker name must be specified.", nameof(brokerName));

            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Client = client ?? throw new ArgumentNullException(nameof(client));
            BrokerName = brokerName;
            RequestQueueKey = settings.RequestQueueKey;
            RequestQueueName = settings.RequestQueueName;
            ContentType = settings.ContentType;

        }
    }
}
