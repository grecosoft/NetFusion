using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Mapping
{
    /// <summary>
    /// Log event categories used when writing to the log.
    /// </summary>
    public class MappingLogEvents
    {
        private const int PluginLog = LogEvents.Common + 100;
        public static EventId MappingException = new EventId(-(PluginLog + 1), "Mapping: Mapping Exception");
        public static EventId MappingApplied = new EventId(PluginLog + 20, "Mapping: Mapping Strategy Applied");
    }
}
