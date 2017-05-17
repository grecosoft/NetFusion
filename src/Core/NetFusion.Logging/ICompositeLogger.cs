namespace NetFusion.Logging
{
    /// <summary>
    /// Logger component that will publish an application's log to a configured
    /// endpoint and route.  
    /// </summary>
    public interface ICompositeLogger
    {
        /// <summary>
        /// Publishes a composite-application host log to configured server.
        /// </summary>
        /// <param name="hostLog">The host log to submit.</param>
        void Log(HostLog hostLog);
    }
}
