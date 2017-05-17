using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core.Initialization;
using System;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Information determined by the message-broker module used to initialize
    /// the message broker used to configure the RabbitMq broker server.
    /// </summary>
    public class MessageBrokerState
    {
        public IConnectionManager ConnectionMgr { get; set; }
        public ISerializationManager SerializationMgr { get; set; }

        public BrokerSettings BrokerSettings { get; set; }
        public IEnumerable<IMessageExchange> Exchanges { get; set; }
        public IDictionary<string, Type> RpcTypes { get; set; }
    }
}
