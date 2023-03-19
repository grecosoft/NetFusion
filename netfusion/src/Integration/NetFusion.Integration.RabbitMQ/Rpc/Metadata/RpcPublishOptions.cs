using NetFusion.Common.Base;

namespace NetFusion.Integration.RabbitMQ.Rpc.Metadata;

public class RpcPublishOptions
{
    public int CancelRpcRequestAfterMs { get; set; } = 5_000;
    public string ContentType { get; set; } = ContentTypes.Json;
    public ushort ResponseQueuePrefetchCount { get; set; } = 1;
}