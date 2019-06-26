using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Microsoft.Extensions.Logging;
using NetFusion.AMQP.Settings;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings;

namespace NetFusion.AMQP.Plugin.Modules
{
    /// <summary>
    /// Module when bootstrapped stores metadata about the defined AMQP connections.
    /// This metadata is used to establish connections and sessions.
    /// </summary>
    public class ConnectionModule : PluginModule,
        IConnectionModule
    {
        private bool _isModuleStopped;
        
        // The configured host settings.
        private AmqpHostSettings _amqpSettings;

        // Sender Settings:
        private static readonly SemaphoreSlim SenderConnSemaphore = new SemaphoreSlim(1,1);
        private readonly Dictionary<string, Connection> _senderConnections;     // Host Name => AMQP connection
        
        // Receiver Settings:
        private readonly Dictionary<string, Connection> _receiverConnections;   // Host Name => AMQP Connection
        private readonly List<Session> _receiverSessions;
        private Action<string> _receiverConnCloseHandler;

        public ConnectionModule()
        {
            _senderConnections = new Dictionary<string, Connection>();
            _receiverConnections = new Dictionary<string, Connection>();
            
            _receiverSessions = new List<Session>();
        }
        
        //------------------------------------------------------
        //--Plugin Initialization
        //------------------------------------------------------
        
        public override void Initialize()
        {
            _amqpSettings = Context.Configuration.GetSettings(Context.Logger, new AmqpHostSettings());
        }
        
        //------------------------------------------------------
        //--Plugin Execution
        //------------------------------------------------------

        // Create connections to the configured hosts to be used for receiving messages.
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            return CreateReceiverConnections();
        }

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            _isModuleStopped = true;
            
            foreach (Session session in _receiverSessions)
            {
                await session.CloseAsync();
            }

            foreach (Connection connection in _receiverConnections.Values)
            {
                await connection.CloseAsync();
            }
        }

        private HostSettings GetHostSettings(string hostName)
        {
            HostSettings host = _amqpSettings.Hosts.FirstOrDefault(h => h.HostName == hostName);
            if (host == null)
            {
                throw new InvalidOperationException(
                    $"Host with the name: {hostName} is not configured.");
            }

            return host;
        }
        
        //--------------------------------------------------------------------
        //-- Sender Connection Services:
        //--------------------------------------------------------------------
        
        public async Task<Session> CreateSenderSession(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentException("Host Name not specified.", nameof(hostName));
           
            if (! HasOpenSenderConnection(hostName))
            {
                await SenderConnSemaphore.WaitAsync();
                try
                {
                    if (! HasOpenSenderConnection(hostName))
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
            HostSettings host = GetHostSettings(hostName);
            
            var address = new Address(host.HostAddress, host.Port, 
                host.Username,
                host.Password);

            var connection =  await Connection.Factory.CreateAsync(address);
            connection.Closed += (sender, error) => { LogItemClosed("Sender Connection", error); };
            
            _senderConnections[hostName] = connection;
        }
        
        //--------------------------------------------------------------------
        //-- Receiver Connection Services:
        //--------------------------------------------------------------------

        public void SetReceiverConnectionCloseHandler(Action<string> handler)
        {
            _receiverConnCloseHandler = handler ?? throw new ArgumentNullException(nameof(handler));
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
        
        private async Task CreateReceiverConnections()
        {
            foreach (HostSettings host in _amqpSettings.Hosts)
            {
                await CreateReceiverHostConnection(host.HostName);
            }
        }

        private async Task CreateReceiverHostConnection(string hostName)
        {
            HostSettings host = GetHostSettings(hostName);
            
            var address = new Address(host.HostAddress, host.Port, 
                host.Username,
                host.Password);

            var connection = await Connection.Factory.CreateAsync(address);
            _receiverConnections[host.HostName] = connection;
            
            connection.Closed += async (sender, error) =>
            {
                LogItemClosed("Receiver Connection", error);
                
                await ReSetReceiverConnection(hostName);
                
                _receiverConnCloseHandler?.Invoke(hostName);
            };
        }       

        private Task ReSetReceiverConnection(string hostName)
        {
            if (_isModuleStopped)
            {
                return Task.CompletedTask;
            }
            
            Context.Logger.LogDebug("Connection to {host} is being reset.", hostName);
            
            Connection hostConn = _receiverConnections[hostName];
            
            _receiverSessions.RemoveAll(rs => rs.Connection == hostConn);
            return CreateReceiverHostConnection(hostName);
        }
        
        private void LogItemClosed(string context, Error error)
        {
            string errorDesc = error?.Description;
            if (errorDesc != null)
            {
                Context.Logger.LogDebug("AMQP Item was closed.  Context: {context}  Error: {error}", context, errorDesc);
            }
            else
            {
                Context.Logger.LogDebug("AMQP Items was closed.");
            }
        }
    }
}