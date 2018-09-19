using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.Redis
{
    /// <summary>
    /// Classifications for Redis log entries.
    /// </summary>
    public class RedisLogEvents
    {
        private const int PluginLog = LogEvents.Integration + 200;
        
        public static EventId ConnException = new EventId(-(PluginLog + 1), "Redis: Connection exception");
        public static EventId PublisherException = new EventId(-(PluginLog + 2), "Redis: Publisher exception");
        public static EventId SubscriberException = new EventId(-(PluginLog + 3), "Redis: Subscriber exception");
        
        public static EventId ConnectionEvent = new EventId(PluginLog + 2, "Redis: Connection Event");
        public static EventId PublisherEvent = new EventId(PluginLog + 2, "Redis: Publisher Event");
        public static EventId SubscriberEvent = new EventId(PluginLog + 3, "Redis: Subscriber Event");
    }
}
