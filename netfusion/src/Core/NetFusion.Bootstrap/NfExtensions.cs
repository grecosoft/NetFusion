using NetFusion.Bootstrap.Logging;

namespace NetFusion.Bootstrap
{
    /// <summary>
    /// Static class containing references to global class instances
    /// initialized at the beginning of the bootstrap process.
    /// </summary>
    public static class NfExtensions
    {
        /// <summary>
        /// Reference to a logger implementation providing extended logging.
        /// </summary>
        public static IExtendedLogger Logger { get; set; } = new NullExtendedLogger();
    }
}