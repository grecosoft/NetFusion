namespace NetFusion.RabbitMQ.Core
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
    }
}
