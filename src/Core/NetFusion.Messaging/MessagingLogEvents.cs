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

        public static EventId MESSAGING_EXCEPTION =     new EventId(-(PluginLog + 1), "Messaging: Message Publish Exception");

        public static EventId MESSAGING_CONFIGURATION = new EventId(PluginLog + 20, "Messaging: Container Configuration");
        public static EventId MESSAGING_DISPATCH =      new EventId(PluginLog + 21, "Messaging: Message Dispatched");
    }
}
