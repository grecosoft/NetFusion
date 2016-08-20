using System;

namespace NetFusion.RabbitMQ.Consumers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RpcConsumerAttribute : Attribute
    {
        public string BrokerName { get; }
        public string RequestQueueKey { get; }
        
        public RpcConsumerAttribute(string brokerName, string requestQueueKey)
        {
            this.BrokerName = brokerName;
            this.RequestQueueKey = requestQueueKey;
        }

        public string ExternalTypeName { get; set; }
    }
}
