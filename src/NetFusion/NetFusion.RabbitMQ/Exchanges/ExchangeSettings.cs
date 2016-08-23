using NetFusion.Domain.Scripting;
using NetFusion.RabbitMQ.Serialization;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Settings to be used when declaring the exchange.
    /// </summary>
    public class ExchangeSettings
    {
        public ExchangeSettings()
        {
            this.Arguments = new Dictionary<string, object>();
            this.ContentType = SerializerTypes.Json;
        }

        /// <summary>
        /// The name of the broker on which the exchange should
        /// be created.
        /// </summary>
        public string BrokerName { get; set; }

        /// <summary>
        /// The type of exchange to be created.
        /// </summary>
        internal string ExchangeType { get; set; }

        /// <summary>
        /// The name of the exchange.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// The content type of the serialized message body.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Indicates that the exchange should survive a server restart.
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// True if the server should delete the exchange when it is no
        /// longer in use.
        /// </summary>
        public bool IsAutoDelete { get; set; }

        public bool IsConsumerExchange { get; set; }

        /// <summary>
        /// Other properties (construction arguments) for the exchange.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Optionally defines the script that should be invoked on the message
        /// to determine if the message should be published to the exchange.
        /// </summary>
        public ScriptPredicate Predicate { get; set; }
    }
}
