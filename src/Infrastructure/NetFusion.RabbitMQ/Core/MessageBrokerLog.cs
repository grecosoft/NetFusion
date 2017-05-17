using NetFusion.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Adds exchange and consumer log information to the module's log.
    /// </summary>
    internal class MessageBrokerLog
    {
        private readonly IEnumerable<IMessageExchange> _exchanges;
        private readonly IEnumerable<MessageConsumer> _messageConsumers;
        
        public MessageBrokerLog(
            IEnumerable<IMessageExchange> exchanges,
            IEnumerable<MessageConsumer> messageConsumers)
        {
            Check.NotNull(exchanges, nameof(exchanges));
            Check.NotNull(messageConsumers, nameof(messageConsumers));

            _exchanges = exchanges;
            _messageConsumers = messageConsumers;
        }

        public void LogMessageExchanges(IDictionary<string, object> moduleLog)
        {
            Check.NotNull(moduleLog, nameof(moduleLog));

            var log = from e in _exchanges.ToLookup(k => k.MessageType)
                select new
                {
                    MessageType = e.Key,
                    Exchanges = from ex in e
                        select new
                        {
                            ex.ExchangeName,
                            ex.Settings,
                            ex.Queues
                        }
                };

            moduleLog["Message-Exchanges"] = log;
        }

        public void LogMessageConsumers(IDictionary<string, object> moduleLog)
        {
            Check.NotNull(moduleLog, nameof(moduleLog));

            var log = from c in _messageConsumers
                select new
                {
                    BindingType = c.BindingType.ToString(),
                    ChannelInfo = LogChannel(c.MessageHandlers.FirstOrDefault()?.Channel),
                    c.ExchangeName,
                    c.QueueName,
                    c.QueueSettings,
                    c.RouteKeys,
                    c.DispatchInfo.MessageHandlerMethod.Name
                };

            moduleLog["Message-Consumers"] = log.ToList();
        }

        public static object LogChannel(IModel channel)
        {
            if (channel == null) return null;

            return new
            {
                channel.ChannelNumber,
                channel.NextPublishSeqNo,

                Conn = LogConnection(channel)
            };
        }

        private static object LogConnection(IModel channel)
        {
            var conn = (channel as Model)?.Session?.Connection;
            if (conn == null) return null;

            return new
            {
                conn.AutoClose,
                conn.ChannelMax,
                conn.Endpoint.HostName,
                conn.Endpoint.Port,
                conn.Heartbeat
            };
        }
    } 
}
