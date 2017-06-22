using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collection;
using NetFusion.RabbitMQ.Configs;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for connecting to the message broker and
    /// creating channels for publishing and consumed queues.
    /// </summary>
    public class ConnectionManager : IConnectionManager,
        IDisposable
    {
        private bool _disposed;

        private readonly ILogger _logger;
        private readonly BrokerSettings _brokerSettings;
        private readonly IDictionary<string, object> _clientProperties;

        private readonly IDictionary<string, BrokerConnection> _connections;

        public ConnectionManager(
            ILoggerFactory loggerFactory,
            BrokerSettings brokerSettings,
            IDictionary<string, object> clientProperties)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(brokerSettings, nameof(brokerSettings));
            Check.NotNull(clientProperties, nameof(clientProperties));

            _logger = loggerFactory.CreateLogger<ConnectionManager>();
            _brokerSettings = brokerSettings;
            _clientProperties = clientProperties;

            _connections = GetConnections(brokerSettings);
        }

        private IDictionary<string, BrokerConnection> GetConnections(BrokerSettings settings)
        {
            if (settings.Connections == null)
            {
                return new Dictionary<string, BrokerConnection>();
            }

            IEnumerable<string> duplicateBrokerNames = settings.Connections
                .WhereDuplicated(c => c.BrokerName);

            if (duplicateBrokerNames.Any())
            {
                throw new BrokerException(
                    $"The following broker names are specified more than " +
                    $"once: {String.Join(", ", duplicateBrokerNames)}.");
            }

            return settings.Connections
                .Select(s => new BrokerConnection(s))
                .ToDictionary(c => c.Settings.BrokerName);
        }

        /// <summary>
        /// Creates connections to all configured brokers.
        /// </summary>
        public void EstablishBrockerConnections()
        {
            foreach (BrokerConnection brokerConn in _connections.Values)
            {
                ConnectToBroker(brokerConn);
            }
        }

        protected virtual void ConnectToBroker(BrokerConnection brokerConn)
        {
            IConnectionFactory connFactory = CreateConnFactory(brokerConn.Settings);

            try
            {
                ExecuteAction.WithRetry<BrokerUnreachableException>(
                    _brokerSettings.NumConnectionRetries,
                     (retryCount) => {
                        LogConnectionException(brokerConn.Settings, retryCount);
                        brokerConn.Connection = connFactory.CreateConnection();
                     });
            }
            catch (BrokerUnreachableException ex)
            {
                LogConnectionException(brokerConn.Settings, connEx: ex);
            }
        }

        // Creates a connection factory with an associated list of key/value pairs
        // that will be associated with the connection.  These values can be viewed
        // within the RabbitMQ Web Administration interface.
        private IConnectionFactory CreateConnFactory(BrokerConnectionSettings brokerSettings)
        {
            IDictionary<string, object> clientProps = AppendConnectionProperties(
                brokerSettings, _clientProperties);

            return new ConnectionFactory
            {
                HostName = brokerSettings.HostName,
                UserName = brokerSettings.UserName,
                Password = brokerSettings.Password,
                VirtualHost = brokerSettings.VHostName,
                ClientProperties = clientProps
            };
        }

        private void LogConnectionException(BrokerConnectionSettings settings,
            int retryCount = 0,
            BrokerUnreachableException connEx = null)
        {
            var connInfo = new { settings.BrokerName, settings.HostName, settings.UserName };
            if (retryCount > 0)
            {
                _logger.LogErrorDetails(RabbitMqLogEvents.BROKER_EXCEPTION,
                "Error connecting to broker. Attempt: {attempt}",
                connInfo, retryCount);
            }

            if (connEx != null)
            {
                _logger.LogErrorDetails(RabbitMqLogEvents.BROKER_EXCEPTION, connEx,
                    "Error connecting to broker.",
                    connInfo);

                throw connEx;
            }
        }

        private IDictionary<string, object> AppendConnectionProperties(
            BrokerConnectionSettings brokerSettings,
            IDictionary<string, object> clientProperties)
        {
            var props = new Dictionary<string, object>(clientProperties);
            props["Broker Name"] = brokerSettings.BrokerName;
            props["Broker User"] = brokerSettings.UserName;
            props["Time Connected"] = DateTime.Now.ToString();

            return props;
        }

        /// <summary>
        /// Creates a new channel on the specified broker.
        /// </summary>
        /// <param name="brokerName">The broker to create channel.</param>
        /// <returns>The created connection channel.</returns>
        public IModel CreateChannel(string brokerName)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));

            BrokerConnection brokerConn = GetBrokerConnection(brokerName);

            IModel channel = null;
            try
            {
                channel = brokerConn.Connection.CreateModel();
            }
            catch (BrokerUnreachableException)
            {
                ConnectToBroker(brokerConn);
                channel = brokerConn.Connection.CreateModel();
            }
            catch (AlreadyClosedException)
            {
                ConnectToBroker(brokerConn);
                channel = brokerConn.Connection.CreateModel();
            }

            return channel;
        }

        /// <summary>
        /// Reestablishes the connection to the broker if not open.
        /// </summary>
        /// <param name="brokerName">The name of the broker.</param>
        public bool ReconnectToBroker(string brokerName)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));

            BrokerConnection brokerConn = GetBrokerConnection(brokerName);
          
            if (brokerConn.Connection == null || !brokerConn.Connection.IsOpen)
            {
                ConnectToBroker(brokerConn);
                return true;
            }
            return false;
        }

        public void CloseConnections()
        {
            foreach (BrokerConnection brokerConn in _connections.Values)
            {
                brokerConn.Connection?.Close();
            }
        }

        /// <summary>
        /// Determines if there is currently an open connection to the broker.
        /// </summary>
        /// <param name="brokerName">The name of the broker.</param>
        /// <returns>True if connection is open.  Otherwise, False.</returns>
        public bool IsBrokerConnected(string brokerName)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));

            BrokerConnection brokerConn = GetBrokerConnection(brokerName);
            return brokerConn.Connection != null && brokerConn.Connection.IsOpen;
        }

        /// <summary>
        /// Determines if the shutdown event represents one that was not expected.
        /// </summary>
        /// <param name="shutdownEvent">The event to test.</param>
        /// <returns>True if unexpected.  Otherwise, False.</returns>
        public bool IsUnexpectedShutdown(ShutdownEventArgs shutdownEvent)
        {
            Check.NotNull(shutdownEvent, nameof(shutdownEvent));

            return shutdownEvent.Initiator == ShutdownInitiator.Library
                || shutdownEvent.Initiator == ShutdownInitiator.Peer;
        }

        private BrokerConnection GetBrokerConnection(string brokerName)
        {
            BrokerConnection brokerConn = _connections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new BrokerException(
                   $"An existing broker with the name of: {brokerName} does not exist.");
            }
            return brokerConn;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;
            if (_connections == null) return;

            foreach(BrokerConnection brokerConn in _connections.Values)
            {
                brokerConn?.Connection.Dispose();
            }

            _disposed = true;
        }
    }
}
