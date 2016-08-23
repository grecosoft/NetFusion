using NetFusion.Messaging;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Implements message based RPC calls that are published to a consumer's
    /// queue.  The consumer responds by publishing the response on a queue
    /// specified by the initiation publisher.
    /// </summary>
    public interface IRpcClient
    {
        /// <summary>
        /// Publishes a command to a remote consumer's RPC defined queue.
        /// </summary>
        /// <param name="command">The command to be published.</param>
        /// <param name="rpcProps">Properties associated with RPC call.</param>
        /// <param name="messageBody">The serialized contents of the message.</param>
        /// <returns>The serialized response returned from the consumer.</returns>
        Task<byte[]> Invoke(ICommand command, RpcProperties rpcProps, byte[] messageBody);
    }
}
