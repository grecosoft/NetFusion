using RabbitMQ.Client;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Implements the logic for connecting to the message broker and
    /// creating channels for publishing and consumed queues.
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// For each configured connection, establishes a connection to the
        /// corresponding broker server.  There will be only one connection
        /// to each broker.  All other functionality requiring a connection
        /// takes place over a channel.
        /// </summary>
        void EstablishBrockerConnections();

        /// <summary>
        /// Creates a channel on a the specified broker connection.  
        /// Several channels can be created without incurring the 
        /// overhead associated with creating a TCP connection.
        /// </summary>
        /// <param name="brokerName">The name of the broker to create channel.</param>
        /// <returns>The created channel.</returns>
        IModel CreateChannel(string brokerName);

        /// <summary>
        /// Determines if the connection shutdown was unexpected.
        /// </summary>
        /// <param name="shutdownEvent">The notification event received describing shutdown.</param>
        /// <returns>True if unexpected.  Otherwise, False.</returns>
        bool IsUnexpectedShutdown(ShutdownEventArgs shutdownEvent);

        /// <summary>
        /// Re-connects to the specified broker if the current connection
        /// is not opened.
        /// </summary>
        /// <param name="brokerName">The name of the configured broker.</param>
        void ReconnectToBroker(string brokerName);

        /// <summary>
        /// Determines if the specified broker connection is established to server.
        /// </summary>
        /// <param name="brokerName">The name of the configured broker.</param>
        /// <returns>True if connected, otherwise, false.</returns>
        bool IsBrokerConnected(string brokerName);

        /// <summary>
        /// Closes all broker connections.
        /// </summary>
        void CloseConnections();
    }
}
