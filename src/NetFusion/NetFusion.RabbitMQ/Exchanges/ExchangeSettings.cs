using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Settings to be used when declaring the exchange.
    /// </summary>
    public class ExchangeSettings
    {
        public static string DefaultContentType = "application/json; charset=utf-8";

        public ExchangeSettings()
        {
            this.Arguments = new Dictionary<string, object>();
            this.ContentType = DefaultContentType;
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

        /// <summary>
        /// Other properties (construction arguments) for the exchange.
        /// </summary>
        public IDictionary<string, object> Arguments { get; }
    }
}
