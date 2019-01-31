using System;
using Amqp;

namespace NetFusion.AMQP.Modules
{
    using NetFusion.AMQP.Settings;

    /// <summary>
    /// Stores the connection and session used to communicate with the host.
    /// </summary>
    public class HostSession
    {
        // The settings read from the host application:
        public HostSettings Settings { get; }
        
        public HostSession(HostSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        /// <summary>
        /// The connection for a defined AMQP host.
        /// </summary>
        public Connection Connection { get; private set; }
        
        /// <summary>
        /// A session created on the connection.
        /// </summary>
        public Session Session { get; private set; }

        public void SetSession(Connection connection, Session session)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }
    }
}