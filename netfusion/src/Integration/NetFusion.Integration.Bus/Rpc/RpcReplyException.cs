namespace NetFusion.Integration.Bus.Rpc;

/// <summary>
/// Exception that is thrown if the RPC reply contains an serialized exception.
/// </summary>
public class RpcReplyException : Exception
{
    public RpcReplyException(string message)
        : base(message)
    {
            
    }
        
    public RpcReplyException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}