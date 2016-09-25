using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
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
        private readonly IContainerLogger _logger;
        private readonly BrokerSettings _brokerSettings;
        private readonly IDictionary<string, object> _clientProperties;

        private readonly IDictionary<string, BrokerConnection> _connections;

        public ConnectionManager(
            IContainerLogger logger,
            BrokerSettings brokerSettings,
            IDictionary<string, object> clientProperties)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(brokerSettings, nameof(brokerSettings));
            Check.NotNull(clientProperties, nameof(clientProperties));

            _logger = logger.ForPluginContext<ConnectionManager>();
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

            return settings.Connections.ToDictionary(c => c.BrokerName);
        }

        public void EstablishBrockerConnections()
        {
            foreach (BrokerConnection brokerConn in _connections.Values)
            {
                ConnectToBroker(brokerConn);
            }
        }

        protected virtual void ConnectToBroker(BrokerConnection brokerConn)
        {
            IConnectionFactory connFactory = CreateConnFactory(brokerConn);

            try
            {
                brokerConn.Connection = connFactory.CreateConnection();
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.Error("Error connecting to broker.", ex,
                    new
                    {
                        brokerConn.BrokerName,
                        brokerConn.HostName,
                        brokerConn.UserName
                    });

                MethodInvoker.TryCallFor<BrokerUnreachableException>(
                    _brokerSettings.ConnectionRetryDelayMs,
                    _brokerSettings.NumConnectionRetries,

                    () => brokerConn.Connection = connFactory.CreateConnection());
            }
        }

        // Creates a connection factory with an associated list of key/value pairs
        // that will be associated with the connection.  These values can be viewed
        // within the RabbitMQ Web Administration interface.
        private IConnectionFactory CreateConnFactory(BrokerConnection brokerConn)
        {
            IDictionary<string, object> clientProps = AppendConnectionProperties(brokerConn, _clientProperties);

            return new ConnectionFactory
            {
                HostName = brokerConn.HostName,
                UserName = brokerConn.UserName,
                Password = brokerConn.Password,
                VirtualHost = brokerConn.VHostName,
                ClientProperties = clientProps
            };
        }

        private IDictionary<string, object> AppendConnectionProperties(
            BrokerConnection brokerConn,
            IDictionary<string, object> clientProperties)
        {
            var props = new Dictionary<string, object>(clientProperties);
            props["Broker Name"] = brokerConn.BrokerName;
            props["Broker User"] = brokerConn.UserName;
            props["Time Connected"] = DateTime.Now.ToString();

            return props;
        }

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

        public void ReconnectToBroker(string brokerName)
        {
            BrokerConnection brokerConn = GetBrokerConnection(brokerName);
          
            if (brokerConn.Connection == null || !brokerConn.Connection.IsOpen)
            {
                ConnectToBroker(brokerConn);
            }
        }

        public bool IsBrokerConnected(string brokerName)
        {
            BrokerConnection brokerConn = GetBrokerConnection(brokerName);
            return brokerConn.Connection != null && brokerConn.Connection.IsOpen;
        }

        public bool IsUnexpectedShutdown(ShutdownEventArgs shutdownEvent)
        {
            return shutdownEvent.Initiator == ShutdownInitiator.Library
                || shutdownEvent.Initiator == ShutdownInitiator.Peer;
        }

        private BrokerConnection GetBrokerConnection(string brokerName)
        {
            BrokerConnection brokerConn = _connections.GetOptionalValue(brokerName);
            if (brokerConn == null)
            {
                throw new BrokerException(
                   $"An existing broker with the name of: {brokerConn} does not exist.");
            }
            return brokerConn;
        }

        public void Dispose()
        {
            //_messageConsumers.ForEach(c => c.Channel?.Dispose());
            _connections.ForEachValue(bc => bc.Connection.Dispose());
        }
    }
}
