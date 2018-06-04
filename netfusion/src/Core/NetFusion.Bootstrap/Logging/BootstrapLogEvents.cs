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

        public static EventId BootstrapException =       new EventId(-(PluginLog + 1), "Bootstrap: Container Exception");

        public static EventId BootstrapInitialize =      new EventId(PluginLog + 20, "Bootstrap: Container Initialization");
        public static EventId BootstrapBuild =           new EventId(PluginLog + 21, "Bootstrap: Container Built");
        public static EventId BootstrapFoundManifests =  new EventId(PluginLog + 22, "Bootstrap: Found Manifests");
        public static EventId BootstrapPluginDetails =   new EventId(PluginLog + 23, "Bootstrap: Plug-In Details");
        public static EventId BootstrapCompositeLog =    new EventId(PluginLog + 24, "Bootstrap: Composite Log");

        public static EventId BootstrapStart =           new EventId(PluginLog + 30, "Bootstrap: Container Started");
        public static EventId BootstrapStop =            new EventId(PluginLog + 31, "Bootstrap: Container Stopped");
    }
}
