using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Container configuration that allows the host application 
    /// to specify the logger to use.  If no logger is specified,
    /// a NullLogger is used.
    /// </summary>
    public class LoggerConfig : IContainerConfig
    {
        /// <summary>
        /// The logger implementation provided by the host application.
        /// </summary>
        public IContainerLogger Logger { get; private set; }

        /// <summary>
        /// Indicates if the application container should log all exceptions before 
        /// they are thrown.  If false, it is the responsibility of the host application.
        /// </summary>
        public bool LogExceptions { get; set; } = true;

        /// <summary>
        /// The logger implementation that will be used by the container
        /// to log container and plug-in specific errors and messages.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public void SetLogger(IContainerLogger logger)
        {
            Check.NotNull(logger, nameof(logger), "logger implementation not specified");

            this.Logger = logger;
        }
    }
}
