using NetFusion.Common.Extensions.Collections;
using NetFusion.RabbitMQ.Exchanges;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Extension methods for RabbitMQ driver IModel (channel) interface based on
    /// classes used by this plug-in.  This simplifies the calling code.
    /// </summary>
    internal static class ChannelExtensions
    {
        public static void ExchangeDeclare(this IModel channel,
            ExchangeSettings settings)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            channel.ExchangeDeclare(settings.ExchangeName, settings.ExchangeType,
                settings.IsDurable,
                settings.IsAutoDelete,
                settings.Arguments);
        }

        public static void QueueDeclare(this IModel channel,
            string queueName,
            QueueSettings settings)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be specified.", nameof(queueName));

            channel.QueueDeclare(queueName,
                settings.IsDurable,
                settings.IsExclusive,
                settings.IsAutoDelete,
                settings.Arguments);
        }

        public static void QueueDeclare(this IModel channel,
            MessageConsumer eventConsumer)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (eventConsumer == null) throw new ArgumentNullException(nameof(eventConsumer));

            if (eventConsumer.IsBrokerAssignedName)
            {
                eventConsumer.QueueName = channel.QueueDeclare().QueueName;
                return;
            }

            channel.QueueDeclare(eventConsumer.QueueName,
                eventConsumer.QueueSettings.IsDurable,
                eventConsumer.QueueSettings.IsExclusive,
                eventConsumer.QueueSettings.IsAutoDelete, null);
        }

        public static void QueueBind(this IModel channel, MessageConsumer eventConsumer)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (eventConsumer == null) throw new ArgumentNullException(nameof(eventConsumer));

            if (eventConsumer.RouteKeys.Empty())
            {
                channel.QueueBind(eventConsumer.QueueName, eventConsumer.ExchangeName, "");
                return;
            }

            foreach(string routeKey in eventConsumer.RouteKeys)
            {
                if (string.IsNullOrWhiteSpace(routeKey))
                {
                    throw new InvalidOperationException(
                        $"The queue named: {eventConsumer.QueueName} on the exchange named: {eventConsumer.ExchangeName} " +
                        $"has a null route-key value specified.");
                }

                channel.QueueBind(eventConsumer.QueueName, eventConsumer.ExchangeName, routeKey.ToUpper());
            }
        }

        public static EventingBasicConsumer GetBasicConsumer(this IModel channel,
            MessageConsumer eventConsumer)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (eventConsumer == null) throw new ArgumentNullException(nameof(eventConsumer));

            var basicConsumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(eventConsumer.QueueName, eventConsumer.QueueSettings, basicConsumer);
            return basicConsumer;
        }

        public static EventingBasicConsumer GetBasicConsumer(this IModel channel, 
            ExchangeQueue queue)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue.QueueName, queue.Settings, consumer);
            return consumer;
        }

        public static void BasicConsume(this IModel channel,
            string queueName,
            QueueSettings settings,
            EventingBasicConsumer basicConsumer)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be specified.", nameof(queueName));

            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (basicConsumer == null) throw new ArgumentNullException(nameof(basicConsumer));

            channel.BasicConsume(queueName, settings.IsNoAck, basicConsumer);
        }
    }
}
