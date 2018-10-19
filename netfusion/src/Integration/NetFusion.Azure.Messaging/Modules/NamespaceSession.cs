using System;
using Amqp;
using NetFusion.Azure.Messaging.Settings;

namespace NetFusion.Azure.Messaging.Modules
{
    /// <summary>
    /// Stores the connection and session used to communicate with an Azure namespace.
    /// </summary>
    public class NamespaceSession
    {
        // The setting read from the host application:
        public NamespaceSettings Settings { get; }
        
        public NamespaceSession(NamespaceSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        /// <summary>
        /// The connection for a defined Azure namespace.
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