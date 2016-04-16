namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Interface that can be implemented by the host application
    /// to log to a specific logger implementation.
    /// </summary>
    public interface IContainerLogger
    {
        bool IsVerboseLevel { get; }
        bool IsDebugLevel { get; }

        void Verbose(string message);
        void Debug(string message);
        void Error(string message);
        void Warning(string message);
    }
}
