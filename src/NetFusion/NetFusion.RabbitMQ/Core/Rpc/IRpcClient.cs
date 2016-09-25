using NetFusion.Messaging;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Rpc
{
    /// <summary>
    /// Implements message based RPC calls that are published to a consumer's
    /// queue.  The consumer responds by publishing the response on a queue
    /// specified by the initiating publisher.
    /// </summary>
    public interface IRpcClient
    {
        /// <summary>
        /// The name of the queue on which replies to responses 
        /// will be published.
        /// </summary>
        string ReplyQueueName { get; }

        /// <summary>
        /// The underlying broker consumer.
        /// </summary>
        EventingBasicConsumer Consumer { get; }

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
