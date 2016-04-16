using RabbitMQ.Client;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Contains properties for connecting to a given message broker.
    /// </summary>
    public class BrokerConnection
    {
        public string BrokerName { get; set; }
        public string HostName { get; set; }
        public string VHostName { get; set; } = "/";
   
        /// <summary>
        /// Additional properties that can be specified for a queue. 
        /// </summary>
        public IEnumerable<QueueProperties> QueueProperties { get; set; }

        // The established connection.
        internal IConnection Connection { get; set; }
    }
}
