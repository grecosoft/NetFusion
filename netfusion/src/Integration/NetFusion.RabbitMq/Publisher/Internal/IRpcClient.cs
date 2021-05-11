using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Describes a component that can send messages to a receiving queue and
    /// monitors an associated reply queue for the corresponding response.
    /// </summary>
    public interface IRpcClient : IDisposable
    {
        /// <summary>
        /// The name of the bus associated with the connection.
        /// </summary>
        public string BusName { get; }
        
        /// <summary>
        /// Should publish the message bytes to the exchange and monitor the reply queue for the response.
        /// </summary>
        /// <param name="createdExchange">Contains the exchange definition to which the message should be published.</param>
        /// <param name="messageBody">The serialize contents of the message.</param>
        /// <param name="messageProperties">Properties associated with the message.</param>
        /// <param name="cancellationToken">Caller specified cancellation token.</param>
        /// <returns>The bytes of the command response.</returns>
        Task<byte[]> Publish(CreatedExchange createdExchange, byte[] messageBody,
            MessageProperties messageProperties,
            CancellationToken cancellationToken);

        /// <summary>
        /// Should create a queue on the default exchange to which the consumer of the
        /// command can publish the response.
        /// </summary>
        Task CreateAndSubscribeToReplyQueueAsync();
    }
}