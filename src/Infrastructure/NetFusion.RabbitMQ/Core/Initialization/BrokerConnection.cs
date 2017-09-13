using NetFusion.RabbitMQ.Configs;
using RabbitMQ.Client;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Associates a broker connection settings with the connection
    /// created based on those settings.
    /// </summary>
    public class BrokerConnection
    {
        /// <summary>
        /// The configured broker connection settings.
        /// </summary>
        public BrokerConnectionSettings Settings { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">The configured broker connection settings.</param>
        public BrokerConnection(BrokerConnectionSettings settings)
        {
            Settings = settings;
        }
        
        /// <summary>
        /// The connection created from the settings.
        /// </summary>
        public IConnection Connection { get; set; }
    }
}
