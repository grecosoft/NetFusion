using System.Collections.Generic;
using NetFusion.RabbitMQ.Core;

namespace NetFusion.RabbitMQ.Integration
{
    
    public class ExchangeMeta
    {
        public string BrokerName { get; set; }
        public string ExchangeName { get; set; }
        public ExchangeSettings Settings { get; set; }
        public ICollection<QueueMeta> QueueMeta { get; set; }
    }
}
