using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Exchanges;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Information determined by the message-broker module used to initialize
    /// the message broker used to configure the RabbitMq broker server.
    /// </summary>
    public class MessageBrokerMetadata
    {
        public BrokerSettings Settings { get; set; }
        public IDictionary<string, BrokerConnection> Connections { get; set; }
        public IEnumerable<IMessageExchange> Exchanges { get; set; }
    }
}
