using NetFusion.RabbitMQ.Core;
using System;

namespace NetFusion.RabbitMQ.Exchanges
{
    public abstract class RpcExchange : BrokerExchange
    {
        public RpcExchange()
        {
            this.Settings.IsConsumerExchange = true;

            QueueSettings.IsDurable = false;
            QueueSettings.IsNoAck = true;   
            QueueSettings.IsExclusive = false; 
            QueueSettings.IsAutoDelete = true;
        }

        internal override void ValidateConfiguration()
        {
            if(! QueueSettings.IsNoAck)
            {
                throw new InvalidOperationException(
                    "IsNoAch must be true for a RPC exchange.");
            }
        }
    }
}
