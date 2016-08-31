using NetFusion.Common;
using NetFusion.Common.Validation;
using NetFusion.RabbitMQ.Core;
using NetFusion.Settings;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Application settings specifying the connections for each broker 
    /// used by the host application.
    /// </summary>
    public class BrokerSettings : AppSettings,
        IObjectValidation
    {
        /// <summary>
        /// List of broker connections populated by host application.
        /// </summary>
        public IEnumerable<BrokerConnection> Connections { get; set; }

        public BrokerSettings()
        {
            this.Connections = new List<BrokerConnection>();
        }

        /// <summary>
        /// The number of milliseconds between attempts to reconnect to the
        public int ConnectionRetryDelayMs { get; set; } = 3000;

        /// <summary>
        /// The number of times a connection attempt should be tried
        /// after waiting for the specified delay.
        /// </summary>
        public int NumConnectionRetries { get; set; } = 10;

        /// <summary>
        /// Applies all externally defined queue properties to all defined
        /// exchange queues.
        /// </summary>
        /// <param name="exchange">Exchange configuration.</param>
        public void ApplyQueueSettings(IMessageExchange exchange)
        {
            Check.NotNull(exchange, nameof(exchange));

            var properties = GetBrokerQueueProperties(exchange.BrokerName);

            foreach (ExchangeQueue queue in exchange.Queues)
            {
                var queueProps = properties.FirstOrDefault(qp => qp.QueueName == queue.QueueName);
                if (queueProps != null)
                {
                    queue.RouteKeys = queue.RouteKeys.Concat(queueProps.RouteKeys)
                        .Distinct().ToArray();
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

            var properties = GetBrokerQueueProperties(consumer.BrokerName);
            var queueProps = properties.FirstOrDefault(qp => qp.QueueName == consumer.QueueName);

            if (queueProps != null)
            {
                consumer.RouteKeys = consumer.RouteKeys.Concat(queueProps.RouteKeys)
                    .Distinct()
                    .ToArray();
            }
        }

        private IEnumerable<QueueProperties> GetBrokerQueueProperties(string brokerName)
        {
            var brokerConn = this.Connections.FirstOrDefault(c => c.BrokerName == brokerName);
            return brokerConn?.QueueProperties ?? new QueueProperties[] { };
        }

        public override ObjectValidator ValidateObject()
        {
            var valResult = base.ValidateObject();

            valResult.Guard(this.ConnectionRetryDelayMs > 0, 
                "Connection Retry Delay must be Greater than 0.", 
                ValidationLevelTypes.Error);

            if (valResult.IsValid)
            {
                IEnumerable<ObjectValidator> connValResults = this.Connections.Select(c => c.ValidateObject());
                valResult.AddChildValidations(connValResults);
            }

            return valResult;
        }
    }
}
