using NetFusion.RabbitMQ.Exchanges;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Integration
{
    /// <summary>
    /// The exchange metadata needed to recreate the exchange.
    /// </summary>
    public class ExchangeMeta
    {
        public string BrokerName { get; set; }
        public string ExchangeName { get; set; }
        public ExchangeSettings Settings { get; set; }
        public ICollection<QueueMeta> QueueMeta { get; set; }
    }
}
