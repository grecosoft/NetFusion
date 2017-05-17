namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Logging event type constants.
    /// </summary>
    public class BootstrapLogEvents
    {
        public const int BOOTSTRAP_EXCEPTION = 1100;

        public const int BOOTSTRAP_INITIALIZE = 1101;
        public const int BOOTSTRAP_BUILD = 1102;
        public const int BOOTSTRAP_FOUND_MANIFESTS = 1103;
        public const int BOOTSTRAP_PLUGIN_DETAILS = 1104;
        public const int BOOTSTRAP_COMPOSITE_LOG = 1105;

        public const int BOOTSTRAP_START = 1106;
        public const int BOOTSTRAP_STOP = 1107;
    }
}
