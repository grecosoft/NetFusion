using NetFusion.Settings;

namespace NetFusion.Logging.Configs
{
    /// <summary>
    /// Settings specifying where a host application should submit its
    /// composite log when bootstrapped.
    /// </summary>
    public class CompositeLogSettings : AppSettings
    {
        /// <summary>
        /// The WebApi endpoint.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// The WebApi route to send log information.
        /// </summary>
        public string LogRoute { get; set; }

        /// <summary>
        /// Indicates if the log should be sent.
        /// </summary>
        public bool SendLog { get; set; } = false;
    }
}
