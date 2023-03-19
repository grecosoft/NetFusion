namespace NetFusion.Integration.RabbitMQ.Rpc.Metadata;

public class RpcQueueMeta
{
    public string QueueName { get; }
    public ushort PrefetchCount { get; set; } = 10;

    public RpcQueueMeta(string queueName)
    {
        QueueName = queueName;
    }
}