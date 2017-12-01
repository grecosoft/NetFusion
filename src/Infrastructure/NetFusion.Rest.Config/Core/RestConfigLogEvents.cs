using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Rest.Config.Core
{
    public static class RestConfigLogEvents
    {
        private const int PluginLog = LogEvents.Infrastructure + 800;

        public static EventId REST_CONFIG_EXCEPTION = new EventId(-(PluginLog + 1), "REST Configuration: Configuration Exception");
    }
}
