using System;
using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ.Plugin
{
    /// <summary>
    /// Event raised when a connection to a broker is reestablished.
    /// </summary>
    public class ReconnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Information about the connection.
        /// </summary>
        public BusConnection Connection { get; set; }
    }
}