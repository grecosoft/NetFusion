using System;
using System.Collections.Generic;
using Amqp;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings;

namespace NetFusion.AMQP.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
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
        
        private readonly Dictionary<string, Connection> _receiverConnections;   // Host Name => AMQP Connection
        private readonly List<Session> _receiverSessions;        

        public ConnectionModule()
        {
            _receiverConnections = new Dictionary<string, Connection>();
            _receiverSessions = new List<Session>();
        }
        
        public override void Initialize()
        {
            _amqpSettings = Context.Configuration.GetSettings(Context.Logger, new AmqpHostSettings());
        }

        // Create connections to the configured hosts to be used for sending and receiving.
        public override void StartModule(IServiceProvider services)
        {
            foreach (HostSettings host in _amqpSettings.Hosts)
            {
                var address = new Address(host.HostAddress, host.Port, 
                    host.Username,
                    host.Password);

                _receiverConnections[host.HostName] = Connection.Factory.CreateAsync(address).Result;
            }
        }
        
        public async Task<Session> CreateSenderSession(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host Name not specified.", nameof(hostName));

            HostSettings host = _amqpSettings.Hosts.FirstOrDefault(h => h.HostName == hostName);
            if (host == null)
            {
                throw new InvalidOperationException(
                    $"Host with the name: {hostName} is not configured.");
            }
            
            var address = new Address(host.HostAddress, host.Port, 
                host.Username,
                host.Password);

            var connection = await Connection.Factory.CreateAsync(address);
            return new Session(connection);
        }

        public Session CreateReceiverSession(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host Name not specified.", nameof(hostName));

            if (! _receiverConnections.TryGetValue(hostName, out Connection connection))
            {
                throw new InvalidOperationException(
                    $"Host with the name: {hostName} is not configured and does not have a connection.");
            }
            
            Session session = new Session(connection);
            
            _receiverSessions.Add(session);
            return session;
        }       
      
        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach (Session session in _receiverSessions)
            {
                session.Close();
            }

            foreach (Connection connection in _receiverConnections.Values)
            {
                connection.Close();
            }
            
            _disposed = true;
        }
    }
}