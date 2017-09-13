using NetFusion.Common;
using System;

namespace NetFusion.RabbitMQ.Core.Rpc
{
    /// <summary>
    /// Exception that is thrown if the RPC reply contains an 
    /// serialized exception.
    /// </summary>
    public class RpcReplyException : Exception
    {
        /// <summary>
        /// The serialized exception returned from the consumer to
        /// which the RPC request was sent.
        /// </summary>
        public byte[] Exception { get; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exception">The serialized RPC reply exception.</param>
        public RpcReplyException(byte[] exception)
        {
            Check.NotNull(exception, nameof(exception));
            Exception = exception;
        }
    }
}
