using System;

namespace NetFusion.RabbitMQ.Core.Rpc
{
    public class RpcReplyException : Exception
    {
        public byte[] Exception { get; }
        
        public RpcReplyException(byte[] exception)
        {
            this.Exception = exception;
        }
    }
}
