using NetFusion.Base.Logging;

namespace NetFusion.Base
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