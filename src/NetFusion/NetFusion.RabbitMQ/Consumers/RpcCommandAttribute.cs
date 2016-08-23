using NetFusion.RabbitMQ.Core;
using System;

namespace NetFusion.RabbitMQ.Consumers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RpcCommandAttribute : Attribute
    {
        public string BrokerName { get; }
        public string RequestQueueKey { get; }
        
        public RpcCommandAttribute(string brokerName, string requestQueueKey)
        {
            this.BrokerName = brokerName;
            this.RequestQueueKey = requestQueueKey;
        }

        public string ExternalTypeName { get; set; }
        public string ContentType { get; set; }

        public RpcProperties ToRpcProps()
        {
            return new RpcProperties
            {
                ContentType = this.ContentType,
                ExternalTypeName = this.ExternalTypeName
            };
        }
    }
}
