using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Common interface implemented by the initializer classes encapsulating a
    /// specific concern for configuring the broker based on the application's
    /// meta-data.
    /// </summary>
    public interface IBrokerInitializer
    {
        /// <summary>
        /// Allows an initialization class to log messages to the bootstrap 
        /// composite application log.
        /// </summary>
        /// <param name="log">Dictionary of log messages.</param>
        void LogDetails(IDictionary<string, object> log);
    }
}
