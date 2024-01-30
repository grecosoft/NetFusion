namespace NetFusion.Integration.RabbitMQ.Rpc.Metadata;

public class RpcQueueMeta(string queueName)
{
    public string QueueName { get; } = queueName;
    public ushort PrefetchCount { get; set; } = 10;
}