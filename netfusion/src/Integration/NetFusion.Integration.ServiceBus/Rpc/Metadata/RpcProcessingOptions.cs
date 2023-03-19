namespace NetFusion.Integration.ServiceBus.Rpc.Metadata;

/// <summary>
/// Options used when processing RPC commands.
/// </summary>
public class RpcProcessingOptions
{
    public int PrefetchCount { get; set; } = 10;
    public int MaxConcurrentCalls { get; set; } = 20;
    public string? Identifier { get; set; }
}