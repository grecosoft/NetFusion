using Microsoft.Extensions.Logging;
using NetFusion.Base;

namespace NetFusion.RabbitMQ
{
    public class RabbitMqLogEvents
    {
        private const int PluginLog = LogEvents.Integration + 100;
        public static EventId BusException = new EventId(-(PluginLog + 1), "RabbitMQ: Bus exception");
        public static EventId PublisherException = new EventId(-(PluginLog + 2), "RabbitMQ: Publisher exception");
        public static EventId SubscriberException = new EventId(-(PluginLog + 3), "RabbitMQ: Subscriber exception");
        public static EventId PublisherEvent = new EventId(PluginLog + 1, "RabbitMQ: Publisher Event");
        public static EventId SubscriberEvent = new EventId(PluginLog + 2, "RabbitMQ: Subscriber Event");
    }
}