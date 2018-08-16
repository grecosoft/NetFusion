using EasyNetQ.Logging;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;

namespace NetFusion.RabbitMQ.Logging
{
    /// <summary>
    /// Configuration that can be added during application container
    /// bootstrapping to delegate EasyNetQ log messages to Microsoft's
    /// ILogger implementation.
    /// 
    /// https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.configuration#bootstrapping---configuration
    /// </summary>
    public class RabbitMqLoggerConfig : IContainerConfig
    {
        /// <summary>
        /// Sets the log factory to which the EasyNetQ logs should be delegated.
        /// </summary>
        /// <param name="loggerFactory">Reference to a configured logger-factory.</param>
        public void SetLogFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new System.ArgumentNullException(nameof(loggerFactory));

            ILogger logger = loggerFactory.CreateLogger("EasyNetQ");

            LogProvider.SetCurrentLogProvider(
                new RabbitMqLogProvider(logger));
        }
    }
}