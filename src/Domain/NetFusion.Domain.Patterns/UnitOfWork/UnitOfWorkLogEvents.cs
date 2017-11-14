using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Domain.Patterns.UnitOfWork
{
    public class UnitOfWorkLogEvents
    {
        private const int PluginLog = LogEvents.Domain + 200;

        public static EventId UOW_COMMIT_EXCEPTION = new EventId(-(PluginLog + 1), "UOW: Commit Exception");

        public static EventId COMMIT_DETAILS = new EventId(PluginLog + 20, "UOW: Commit Details");
        public static EventId INTEGRATION_DETAILS = new EventId(PluginLog + 21, "UOW: Integration Details");
    }
}
