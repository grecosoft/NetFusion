using NetFusion.Base;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Logging event type constants.
    /// </summary>
    public class BootstrapLogEvents
    {
        private const int PluginLog = LogEvents.Core + 100;

        public const int BOOTSTRAP_EXCEPTION = -(PluginLog + 1);

        public const int BOOTSTRAP_INITIALIZE = PluginLog + 10;
        public const int BOOTSTRAP_BUILD = PluginLog + 11;
        public const int BOOTSTRAP_FOUND_MANIFESTS = PluginLog + 12;
        public const int BOOTSTRAP_PLUGIN_DETAILS = PluginLog + 13;
        public const int BOOTSTRAP_COMPOSITE_LOG = PluginLog + 14;

        public const int BOOTSTRAP_START = PluginLog + 20;
        public const int BOOTSTRAP_STOP = PluginLog + 21;
    }
}
