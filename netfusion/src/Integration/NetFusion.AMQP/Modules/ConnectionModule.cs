using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings;

namespace NetFusion.AMQP.Modules
{
    using NetFusion.AMQP.Publisher.Internal;
    using NetFusion.AMQP.Settings;

    /// <summary>
    /// Module when bootstrapped stores metadata about the defined AMQP connections.
    /// This metadata is used to establish connections and sessions.
    /// </summary>
    public class ConnectionModule : PluginModule,
        IConnectionModule
    {
        private bool _disposed;
        private readonly SemaphoreSlim _connMutex = new SemaphoreSlim(1);
        private readonly object _senderLock = new object();
       
        // The configured host settings and a lookup of the session
        // associated with a given host.
        private AmqpHostSettings _amqpSettings;
        private Dictionary<string, HostSession> _sessions;    // Host Name => AMQP Session

        public ConnectionModule()
        {
            _sessions = new Dictionary<string, HostSession>(); 
        }
        
        public override void Initialize()
        {
            _amqpSettings = Context.Configuration.GetSettings(Context.Logger, new AmqpHostSettings());

            _sessions = _amqpSettings.Hosts.ToDictionary(
                c => c.HostName, 
                ns => new HostSession(ns));
        }
        
        // Creates the a connection and session to the host the first time requested.
        public async Task<Session> GetSession(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host Name not specified.", nameof(hostName));
            
            if (! _sessions.TryGetValue(hostName, out HostSession nsSession))
            {
                throw new InvalidOperationException(
                    $"Host with the name: {hostName} is not configured.");
            }

            if (nsSession.Connection == null)
            {
                await _connMutex.WaitAsync().ConfigureAwait(false);
                
                if (nsSession.Connection == null)
                {
                    try
                    {
                        await CreateSession(nsSession);
                    }
                    finally
                    {
                        _connMutex.Release();
                    }    
                }
            }

            return nsSession.Session;
        }

        // Given a session and a configured host item (Queue/Topic) to which message
        // will be sent, initializes the associated AMQP sender link used to send messages.
        public void SetSenderLink(Session session, ISenderHostItem senderHostItem)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (senderHostItem == null) throw new ArgumentNullException(nameof(senderHostItem));

            if (senderHostItem.SenderLink != null)
            {
                return;
            }

            lock (_senderLock)
            {
                if (senderHostItem.SenderLink == null)
                {
                    senderHostItem.SenderLink = new SenderLink(session, 
                        Guid.NewGuid().ToString(), 
                        senderHostItem.Name);
                }
            }
        }
        
        // Creates the AMQP connection and session instances for configured host.
        private static async Task CreateSession(HostSession hostSession)
        {
            HostSettings settings = hostSession.Settings;
            
            var address = new Address(settings.HostAddress, settings.Port, 
                    settings.Username,
                    settings.Password);

            Connection connection = await Connection.Factory.CreateAsync(address);
            Session session = new Session(connection);
                
            // Store the connection and session for future calls.
            hostSession.SetSession(connection, session);
        }

        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(HostSession sessionConn in _sessions.Values)
            {
                sessionConn?.Session?.Close();
                sessionConn?.Connection?.Close();
            }

            _disposed = true;
        }
    }
}