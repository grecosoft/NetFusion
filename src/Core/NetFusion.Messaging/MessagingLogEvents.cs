using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Log event categories used when writing to the log.
    /// </summary>
    public class MessagingLogEvents
    {
        private const int PluginLog = LogEvents.Core + 200;
        public static EventId MessagingException =     new EventId(-(PluginLog + 1), "Messaging: Message Publish Exception");
        public static EventId MessagingConfiguration = new EventId(PluginLog + 20, "Messaging: Container Configuration");
        public static EventId MessagingDispatch =      new EventId(PluginLog + 21, "Messaging: Message Dispatched");
        public static EventId QueryDispatch =          new EventId(PluginLog + 30, "Messaging: Query Dispatched");
    }
}
