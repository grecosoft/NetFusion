using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Roslyn
{
    /// <summary>
    /// Logging event type constants.
    /// </summary>
    public class ScriptingLogEvents
    {
        private const int PluginLog = LogEvents.Integration + 200;

        public static EventId ScriptException =        new EventId(-(PluginLog + 1), "Roslyn: Script Exception");

        public static EventId ScriptExecution =        new EventId(PluginLog + 20, "Roslyn: Script Execution");
        public static EventId ScriptPreEvaluation =   new EventId(PluginLog + 20, "Roslyn: Script Pre-Evaluation");
        public static EventId ScriptPostEvaluation =  new EventId(PluginLog + 20, "Roslyn: Script Post-Evaluation");
    }
}
