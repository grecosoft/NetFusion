using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Azure.Messaging.Settings;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings;

namespace NetFusion.Azure.Messaging.Modules
{
    /// <summary>
    /// Module when bootstrapped stores metadata about the defined service bus
    /// namespace connections.  This metadata is used to establish connections
    /// and sessions using AMQP related classes.
    /// </summary>
    public class ConnectionModule : PluginModule,
        IConnectionModule
    {
        private bool _disposed;
        private readonly SemaphoreSlim _connMutex = new SemaphoreSlim(1);
        private readonly object _senderLock = new object();
       
        // The configured namespace connections and a lookup of the
        // session associated with a given namespace.
        private ServiceBusSettings _serviceBusSettings;
        private Dictionary<string, NamespaceSession> _sessions;

        public ConnectionModule()
        {
            _sessions = new Dictionary<string, NamespaceSession>(); // Namespace Name => Session
        }
        
        public override void Initialize()
        {
            _serviceBusSettings = Context.Configuration.GetSettings(Context.Logger, new ServiceBusSettings());

            _sessions = _serviceBusSettings.Namespaces.ToDictionary(
                c => c.Namespace, 
                ns => new NamespaceSession(ns));
        }
        
        // Creates the a connection and session to the Azure namespace the first time requested.
        public async Task<Session> GetSession(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace Name not specified.", nameof(namespaceName));
            
            if (! _sessions.TryGetValue(namespaceName, out NamespaceSession nsSession))
            {
                throw new InvalidOperationException(
                    $"Namespace with the name: {namespaceName} is not configured.");
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

        public NamespaceSettings GetNamespaceSettings(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace Name not specified.", nameof(namespaceName));

            var nsSettings = _serviceBusSettings.Namespaces.FirstOrDefault(ns => ns.Namespace == namespaceName);
            if (nsSettings == null)
            {
                throw new InvalidOperationException(
                    $"Namespace with the name: {namespaceName} is not configured.");
            }

            return nsSettings;
        }

        // Given a session and a configured namespace to which message will be sent,
        // initializes the associated AMQP sender link used to send messages.
        public void SetSenderLink(Session session, ILinkedItem nsSender)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (nsSender == null) throw new ArgumentNullException(nameof(nsSender));

            if (nsSender.SenderLink != null)
            {
                return;
            }

            lock (_senderLock)
            {
                if (nsSender.SenderLink == null)
                {
                    nsSender.SenderLink = new SenderLink(session, 
                        Guid.NewGuid().ToString(), 
                        nsSender.Name);
                }
            }
        }
        
        // Creates the AMQP connection and session instances for configured namespace.
        private static async Task CreateSession(NamespaceSession nsSession)
        {
            NamespaceSettings settings = nsSession.Settings;
            
            var address = new Address(settings.Namespace, settings.Port, 
                    settings.PolicyName,
                    settings.NamespaceKey);

            Connection connection = await Connection.Factory.CreateAsync(address);
            Session session = new Session(connection);
                
            // Store the connection and session for future calls.
            nsSession.SetSession(connection, session);
        }

        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(NamespaceSession sessionConn in _sessions.Values)
            {
                sessionConn?.Session?.Close();
                sessionConn?.Connection?.Close();
            }

            _disposed = true;
        }
    }
}