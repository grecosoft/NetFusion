using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Domain.Roslyn
{
    /// <summary>
    /// Logging event type constants.
    /// </summary>
    public class ScriptingLogEvents
    {
        private const int PluginLog = LogEvents.Integration + 200;

        public static EventId SCRIPT_EXCEPTION =        new EventId(-(PluginLog + 1), "Roslyn: Script Exception");

        public static EventId SCRIPT_EXECUTION =        new EventId(PluginLog + 20, "Roslyn: Script Execution");
        public static EventId SCRIPT_PRE_EVALUATION =   new EventId(PluginLog + 20, "Roslyn: Script Pre-Evaluation");
        public static EventId SCRIPT_POST_EVALUATION =  new EventId(PluginLog + 20, "Roslyn: Script Post-Evaluation");
    }
}
