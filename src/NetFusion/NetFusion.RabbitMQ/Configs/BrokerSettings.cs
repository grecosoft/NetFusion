﻿using NetFusion.Common;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Exchanges;
using NetFusion.Settings;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.RabbitMQ.Configs
{
    /// <summary>
    /// Application settings specifying the connections for each broker 
    /// used by the host application.
    /// </summary>
    public class BrokerSettings : AppSettings
    {
        /// <summary>
        /// List of broker connections populated by host application.
        /// </summary>
        public IEnumerable<BrokerConnection> Connections { get; set; }

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
    }
}