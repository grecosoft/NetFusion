using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Logging event type constants for the container bootstrap process.
    /// </summary>
    public class BootstrapLogEvents
    {
        private const int PluginLog = LogEvents.Core + 100;

        public static EventId BOOTSTRAP_EXCEPTION =       new EventId(-(PluginLog + 1), "Bootstrap: Container Exception");

        public static EventId BOOTSTRAP_INITIALIZE =      new EventId(PluginLog + 20, "Bootstrap: Container Initialization");
        public static EventId BOOTSTRAP_BUILD =           new EventId(PluginLog + 21, "Bootstrap: Container Built");
        public static EventId BOOTSTRAP_FOUND_MANIFESTS = new EventId(PluginLog + 22, "Bootstrap: Found Manifests");
        public static EventId BOOTSTRAP_PLUGIN_DETAILS =  new EventId(PluginLog + 23, "Bootstrap: Plug-In Details");
        public static EventId BOOTSTRAP_COMPOSITE_LOG =   new EventId(PluginLog + 24, "Bootstrap: Composite Log");

        public static EventId BOOTSTRAP_START =           new EventId(PluginLog + 30, "Bootstrap: Container Started");
        public static EventId BOOTSTRAP_STOP =            new EventId(PluginLog + 31, "Bootstrap: Container Stopped");
    }
}
