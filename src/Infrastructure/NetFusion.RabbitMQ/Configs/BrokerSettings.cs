using NetFusion.Common;
using NetFusion.RabbitMQ.Core;
using NetFusion.Settings;
using NetFusion.Utilities.Validation;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Application settings specifying the connections for each broker 
    /// used by the host application.
    /// </summary>
    [ConfigurationSection("netfusion:plugins:rabbitMQ")]
    public class BrokerSettings : AppSettings,
        IValidatableType
    {
        /// <summary>
        /// List of broker connections populated by host application.
        /// </summary>
        public IList<BrokerConnectionSettings> Connections { get; set; }

        public BrokerSettings()
        {
            Connections = new List<BrokerConnectionSettings>();
        }

        /// <summary>
        /// The number of times a connection attempt should be tried
        /// after waiting for the specified delay.
        /// </summary>
        public int NumConnectionRetries { get; set; } = 5;

        /// <summary>
        /// Returns a configured connection.
        /// </summary>
        /// <param name="brokerName">The name of the connection to search.</param>
        /// <returns>The found connection configuration or an exception.</returns>
        public BrokerConnectionSettings GetConnection(string brokerName)
        {
            Check.NotNull(brokerName, nameof(brokerName));

            BrokerConnectionSettings conn = Connections.FirstOrDefault(c => c.BrokerName == brokerName);
            
            if (conn == null)
            {
                throw new BrokerException($"Connection with broker name: {brokerName} not configured.");
            }
            return conn;
        }

        /// <summary>
        /// Applies all externally defined queue properties to all defined exchange queues.
        /// </summary>
        /// <param name="exchange">Exchange configuration.</param>
        public void ApplyQueueSettings(IMessageExchange exchange)
        {
            Check.NotNull(exchange, nameof(exchange));

            foreach (QueuePropertiesSettings queueProps in GetBrokerQueueProperties(exchange.BrokerName))
            {
                ExchangeQueue queue = exchange.Queues.FirstOrDefault(q => q.QueueName == queueProps.QueueName);
                if (queue != null)
                {
                    queue.RouteKeys = queueProps.RouteKeys.ToArray();
                }
            }
        }

        /// <summary>
        /// Applies all externally defined queue properties to a consumer's queue.
        /// </summary>
        /// <param name="consumer">The consumer configuration.</param>
        public void ApplyQueueSettings(MessageConsumer consumer)
        {
            Check.NotNull(consumer, nameof(consumer));

            IEnumerable<QueuePropertiesSettings> properties = GetBrokerQueueProperties(consumer.BrokerName);
            QueuePropertiesSettings queueProps = properties.FirstOrDefault(qp => qp.QueueName == consumer.QueueName);

            if (queueProps != null)
            {
                consumer.RouteKeys = queueProps.RouteKeys.ToArray();
            }
        }

        private IEnumerable<QueuePropertiesSettings> GetBrokerQueueProperties(string brokerName)
        {
            BrokerConnectionSettings brokerConn = Connections.FirstOrDefault(c => c.BrokerName == brokerName);
            return brokerConn?.QueueProperties ?? new QueuePropertiesSettings[] { };
        }

        public void Validate(IObjectValidator validator)
        {
            validator.Validate(this.NumConnectionRetries > 0,
                "Number Connection Retries must be Greater than 0.", ValidationTypes.Error);

            if (validator.IsValid)
            {
                Connections.Select(c => validator.AddChildValidator(c));
            }
        }
    }
}
