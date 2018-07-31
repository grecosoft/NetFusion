using System;

namespace NetFusion.RabbitMQ.Publisher.Internal
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
        public byte[] ReplayExceptionBody { get; }
        
        public RpcReplyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exceptionBody">The serialized RPC reply exception.</param>
        public RpcReplyException(byte[] exceptionBody)
        {
            ReplayExceptionBody = exceptionBody ?? throw new ArgumentNullException(nameof(exceptionBody));
        }
    }
}
