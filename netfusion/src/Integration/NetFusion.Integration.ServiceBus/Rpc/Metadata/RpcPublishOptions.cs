using NetFusion.Common.Base;

namespace NetFusion.Integration.ServiceBus.Rpc.Metadata;

/// <summary>
/// Options used when sending a RPC command to a queue.
/// </summary>
public class RpcPublishOptions
{
    public int CancelRpcRequestAfterMs { get; set; } = 10_000;
    public string ContentType { get; set; } = ContentTypes.Json;
}