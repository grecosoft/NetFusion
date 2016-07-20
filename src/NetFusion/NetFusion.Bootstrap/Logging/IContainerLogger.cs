namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Interface that can be implemented by the host application
    /// to log to a specific logger implementation.
    /// </summary>
    public interface IContainerLogger
    {
        /// <summary>
        /// Indicates if the verbose level is enabled.
        /// </summary>
        bool IsVerboseLevel { get; }

        /// <summary>
        /// Indicates if the debug level is enabled.
        /// </summary>
        bool IsDebugLevel { get; }

        /// <summary>
        /// Associates a context with the log message.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <returns>New instance of the logger with the configured context.</returns>
        IContainerLogger ForContext<TContext>();

        void Verbose(string message);
        void Debug(string message, object details = null);
        void Error(string message);
        void Warning(string message);
    }
}
