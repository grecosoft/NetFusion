using System;
using System.Collections.Generic;
using Amqp;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amqp.Framing;
using Microsoft.Extensions.Logging;
using NetFusion.AMQP.Settings;

namespace NetFusion.AMQP.Modules
{

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

        private static readonly SemaphoreSlim SenderConnSemaphore = new SemaphoreSlim(1,1);
        private readonly Dictionary<string, Connection> _senderConnections;     // Host Name => AMQP connection
        private readonly Dictionary<string, Connection> _receiverConnections;   // Host Name => AMQP Connection
        private readonly List<Session> _receiverSessions;        

        public ConnectionModule()
        {
            _senderConnections = new Dictionary<string, Connection>();
            _receiverConnections = new Dictionary<string, Connection>();
            
            _receiverSessions = new List<Session>();
        }
        
        public override void Initialize()
        {
            _amqpSettings = Context.Configuration.GetSettings(Context.Logger, new AmqpHostSettings());
        }

        // Create connections to the configured hosts to be used for receiving messages.
        public override void StartModule(IServiceProvider services)
        {
            foreach (HostSettings host in _amqpSettings.Hosts)
            {
                var address = new Address(host.HostAddress, host.Port, 
                    host.Username,
                    host.Password);

                var connection = Connection.Factory.CreateAsync(address).Result;
                connection.Closed += (sender, error) => { LogItemClosed("Receiver Connection", error); };
                
                _receiverConnections[host.HostName] = connection;
            }
        }
        
        public async Task<Session> CreateSenderSession(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host Name not specified.", nameof(hostName));
           
            if (!HasOpenSenderConnection(hostName))
            {
                await SenderConnSemaphore.WaitAsync();
                try
                {
                    if (!HasOpenSenderConnection(hostName))
                    {
                        await CreateSenderConnection(hostName);
                    }
                }
                finally
                {
                    SenderConnSemaphore.Release();
                }
            }
            
            return new Session(_senderConnections[hostName]);
        }

        private bool HasOpenSenderConnection(string hostName)
        {
            if (_senderConnections.TryGetValue(hostName, out var hostConn))
            {
                return !hostConn.IsClosed;
            }

            return false;
        }

        private async Task CreateSenderConnection(string hostName)
        {
            HostSettings host = _amqpSettings.Hosts.FirstOrDefault(h => h.HostName == hostName);
            if (host == null)
            {
                throw new InvalidOperationException(
                    $"Host with the name: {hostName} is not configured.");
            }
            
            var address = new Address(host.HostAddress, host.Port, 
                host.Username,
                host.Password);

            var connection =  await Connection.Factory.CreateAsync(address);
            connection.Closed += (sender, error) => { LogItemClosed("Sender Connection", error); };
            
            _senderConnections[hostName] = connection;
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
            session.Closed += (sender, error) => { LogItemClosed("Receiver Session", error); };
            
            _receiverSessions.Add(session);
            return session;
        }

        private void LogItemClosed(string context, Error error)
        {
            string errorDesc = error?.Description;
            if (errorDesc != null)
            {
                Context.Logger.LogWarning("AMQP Item was closed.  Context: {context}  Error: {error}", context, errorDesc);
            }
            else
            {
                Context.Logger.LogWarning("AMQP Items was closed.");
            }
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