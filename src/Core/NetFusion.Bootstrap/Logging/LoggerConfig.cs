using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using System;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Container configuration that allows the host application 
    /// to specify the ILoggerFactory to be used.  If the not
    /// configured, a default ILoggerFactory configured instance
    /// is used.
    /// </summary>
    public class LoggerConfig : IContainerConfig
    {
        /// <summary>
        /// The logger implementation provided by the host application.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; private set; }

        /// <summary>
        /// Indicates if the application container should log all exceptions before 
        /// they are thrown.  If false, it is the responsibility of the host application.
        /// </summary>
        public bool LogExceptions { get; set; } = true;

        /// <summary>
        /// The logger implementation that will be used by the container
        /// to log container and plug-in specific errors and messages.
        /// </summary>
        /// <param name="loggerFactory">The logger instance.</param>
        public void UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory ?? 
                throw new ArgumentNullException(nameof(loggerFactory), "Logger factory implementation cannot be null.");
        }
    }
}
