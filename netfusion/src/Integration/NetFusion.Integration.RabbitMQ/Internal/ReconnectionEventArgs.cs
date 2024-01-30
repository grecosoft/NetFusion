using NetFusion.Integration.RabbitMQ.Plugin.Settings;

namespace NetFusion.Integration.RabbitMQ.Internal
{
    /// <summary>
    /// Event raised when a connection to a broker is reestablished.
    /// </summary>
    public class ReconnectionEventArgs(ConnectionSettings connection) : EventArgs
    {
        /// <summary>
        /// Information about the connection.
        /// </summary>
        public ConnectionSettings Connection { get; } = connection;
    }
}