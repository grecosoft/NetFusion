using System;
using System.Collections.Generic;
using Amqp;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings;

namespace NetFusion.AMQP.Modules
{
    using System.Collections.Concurrent;
    using NetFusion.AMQP.Settings;

    /// <summary>
    /// Module when bootstrapped stores metadata about the defined AMQP connections.
    /// This metadata is used to establish connections and sessions.
    /// </summary>
    public class ConnectionModule : PluginModule,
        IConnectionModule
    {
        private bool _disposed;
       
        // The configured host settings and a lookup of the session
        // associated with a given host.
        private AmqpHostSettings _amqpSettings;
        private readonly Dictionary<string, Connection> _connections;   // Host Name => AMQP Connection
        private readonly List<Session> _sessions;        
        private readonly ConcurrentDictionary<string, Session> _senderSessions; // Host Name => Session

        public ConnectionModule()
        {
            _connections = new Dictionary<string, Connection>();
            _sessions = new List<Session>();
            _senderSessions = new ConcurrentDictionary<string, Session>();
        }
        
        public override void Initialize()
        {
            _amqpSettings = Context.Configuration.GetSettings(Context.Logger, new AmqpHostSettings());
        }

        // Create connections to the configured hosts.
        public override void StartModule(IServiceProvider services)
        {
            foreach (HostSettings host in _amqpSettings.Hosts)
            {
                var address = new Address(host.HostAddress, host.Port, 
                    host.Username,
                    host.Password);

                Connection connection = Connection.Factory.CreateAsync(address).Result;
                _connections[host.HostName] = connection;
            }
        }

        // Creates a session on the connection with the specified host name.
        public Session CreateSession(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host Name not specified.", nameof(hostName));

            if (! _connections.TryGetValue(hostName, out Connection connection))
            {
                throw new InvalidOperationException(
                    $"Host with the name: {hostName} is not configured and does not have a connection.");
            }
            
            Session session = new Session(connection);
            
            _sessions.Add(session);
            return session;
        }

        public Session GetSenderSession(string hostName)
        {
            return _senderSessions.GetOrAdd(hostName, CreateSession);
        }
      
        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach (Session session in _sessions)
            {
                session.Close();
            }

            foreach (Connection connection in _connections.Values)
            {
                connection.Close();
            }
            
            _disposed = true;
        }
    }
}