using NetFusion.RabbitMQ.Core;
using System;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Used by a consumer to declare and exchange and queues that can have RPC style
    /// messages published.  Publishers specify within their RpcConsumerSettings a key
    /// that is tied to the queue's name.  Then when the publisher posts an command 
    /// marked with the RpcCommandAttribute having the specified key, it will be sent
    /// to this queue and they can await an asynchronous response on their reply queue.
    /// </summary>
    public abstract class RpcExchange : MessageExchange
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
            if(!QueueSettings.IsNoAck)
            {
                throw new InvalidOperationException(
                    "IsNoAch must be true for a RPC exchange.");
            }
        }
    }
}
