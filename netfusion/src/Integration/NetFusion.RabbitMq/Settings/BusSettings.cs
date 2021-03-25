using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Configuration settings for defined buses used by the application.
    /// </summary>
    [ConfigurationSection("netfusion:rabbitMQ")]
    public class BusSettings : IAppSettings,
        IValidatableType
    {
        /// <summary>
        /// List of broker connections specified by host application.
        /// </summary>
        public Dictionary<string, BusConnection> Connections { get; set; } = new();

        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Connections);
        }

        /// <summary>
        /// The configuration represents a collection of items by keyed named.
        /// Updates the name on each item to that of the key specified within
        /// the configuration.
        /// </summary>
        public void SetNamedConfigurations()
        {
            foreach (var (busName, conn) in Connections)
            {
                conn.BusName = busName;
                foreach (var (exchangeName, exchange) in conn.ExchangeSettings)
                {
                    exchange.ExchangeName = exchangeName;
                }

                foreach (var (queueName, queue) in conn.QueueSettings)
                {
                    queue.QueueName = queueName;
                }
            }
        }
    }
}